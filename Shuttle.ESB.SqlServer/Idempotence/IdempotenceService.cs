using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.ESB.Core;

namespace Shuttle.ESB.SqlServer.Idempotence
{
	public class IdempotenceService :
		IIdempotenceService,
		IRequireInitialization
	{
		private readonly string _idempotenceConnectionStringName;

		private readonly IDatabaseGateway _databaseGateway;
		private readonly IDatabaseContextFactory _databaseContextFactory;
		private readonly IScriptProvider _scriptProvider;

		public static IIdempotenceService Default()
		{
			var configuration = SqlServerSection.Configuration();

			return
				new IdempotenceService(configuration,
					new ScriptProvider(configuration),
					DatabaseContextFactory.Default(),
					new DatabaseGateway());
		}


		public IdempotenceService(
			ISqlServerConfiguration configuration,
			IScriptProvider scriptProvider,
			IDatabaseContextFactory databaseContextFactory,
			IDatabaseGateway databaseGateway)
		{
			Guard.AgainstNull(configuration, "configuration");
			Guard.AgainstNull(scriptProvider, "scriptProvider");
			Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
			Guard.AgainstNull(databaseGateway, "databaseGateway");

			_scriptProvider = scriptProvider;
			_databaseContextFactory = databaseContextFactory;
			_databaseGateway = databaseGateway;

			_idempotenceConnectionStringName = configuration.IdempotenceServiceConnectionStringName;
		}

		public void Initialize(IServiceBus bus)
		{
			using (var connection = _databaseContextFactory.Create(_idempotenceConnectionStringName))
			using (var transaction = connection.BeginTransaction())
			{
				if (_databaseGateway.GetScalarUsing<int>(
					RawQuery.Create(
						_scriptProvider.GetScript(
							Script.IdempotenceServiceExists))) != 1)
				{
					throw new IdempotenceServiceException(SqlResources.IdempotenceDatabaseNotConfigured);
				}

				_databaseGateway.ExecuteUsing(
					RawQuery.Create(
						_scriptProvider.GetScript(
							Script.IdempotenceInitialize))
						.AddParameterValue(IdempotenceColumns.InboxWorkQueueUri, bus.Configuration.Inbox.WorkQueue.Uri.ToString()));

				transaction.CommitTransaction();
			}
		}

		public ProcessingStatus ProcessingStatus(TransportMessage transportMessage)
		{
			try
			{
				using (var connection = _databaseContextFactory.Create(_idempotenceConnectionStringName))
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						if (_databaseGateway.GetScalarUsing<int>(
							RawQuery.Create(
								_scriptProvider.GetScript(
									Script.IdempotenceHasCompleted))
								.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)) == 1)
						{
							return Core.ProcessingStatus.Ignore;
						}

						if (_databaseGateway.GetScalarUsing<int>(
							RawQuery.Create(
								_scriptProvider.GetScript(
									Script.IdempotenceIsProcessing))
								.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)) == 1)
						{
							return Core.ProcessingStatus.Ignore;
						}

						_databaseGateway.ExecuteUsing(
							RawQuery.Create(
								_scriptProvider.GetScript(
									Script.IdempotenceProcessing))
								.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)
								.AddParameterValue(IdempotenceColumns.InboxWorkQueueUri, transportMessage.RecipientInboxWorkQueueUri)
								.AddParameterValue(IdempotenceColumns.AssignedThreadId, Thread.CurrentThread.ManagedThreadId));

						var messageHandled = _databaseGateway.GetScalarUsing<int>(
							RawQuery.Create(
								_scriptProvider.GetScript(
									Script.IdempotenceIsMessageHandled))
								.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)) == 1;

						return messageHandled
							? Core.ProcessingStatus.MessageHandled
							: Core.ProcessingStatus.Assigned;
					}
					finally
					{
						transaction.CommitTransaction();
					}
				}
			}
			catch (SqlException ex)
			{
				var message = ex.Message.ToUpperInvariant();

				if (message.Contains("VIOLATION OF UNIQUE KEY CONSTRAINT") || message.Contains("CANNOT INSERT DUPLICATE KEY") || message.Contains("IGNORE MESSAGE PROCESSING"))
				{
					return Core.ProcessingStatus.Ignore;
				}

				throw;
			}
		}

		public void ProcessingCompleted(TransportMessage transportMessage)
		{
			using (var connection = _databaseContextFactory.Create(_idempotenceConnectionStringName))
			using (var transaction = connection.BeginTransaction())
			{
				_databaseGateway.ExecuteUsing(
					RawQuery.Create(
						_scriptProvider.GetScript(Script.IdempotenceComplete))
						.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId));

				transaction.CommitTransaction();
			}
		}

		public void AddDeferredMessage(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage,
			Stream deferredTransportMessageStream)
		{
			using (_databaseContextFactory.Create(_idempotenceConnectionStringName))
			{
				_databaseGateway.ExecuteUsing(
					RawQuery.Create(_scriptProvider.GetScript(Script.IdempotenceSendDeferredMessage))
						.AddParameterValue(IdempotenceColumns.MessageId, deferredTransportMessage.MessageId)
						.AddParameterValue(IdempotenceColumns.MessageIdReceived, processingTransportMessage.MessageId)
						.AddParameterValue(IdempotenceColumns.MessageBody, deferredTransportMessageStream.ToBytes()));
			}
		}

		public IEnumerable<Stream> GetDeferredMessages(TransportMessage transportMessage)
		{
			var result = new List<Stream>();

			using (_databaseContextFactory.Create(_idempotenceConnectionStringName))
			{
				var rows = _databaseGateway.GetRowsUsing(
					RawQuery.Create(_scriptProvider.GetScript(Script.IdempotenceGetDeferredMessages))
						.AddParameterValue(IdempotenceColumns.MessageIdReceived, transportMessage.MessageId));

				foreach (var row in rows)
				{
					result.Add(new MemoryStream((byte[]) row["MessageBody"]));
				}
			}

			return result;
		}

		public void DeferredMessageSent(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage)
		{
			using (_databaseContextFactory.Create(_idempotenceConnectionStringName))
			{
				_databaseGateway.ExecuteUsing(
					RawQuery.Create(_scriptProvider.GetScript(Script.IdempotenceDeferredMessageSent))
						.AddParameterValue(IdempotenceColumns.MessageId, deferredTransportMessage.MessageId));
			}
		}

		public void MessageHandled(TransportMessage transportMessage)
		{
			using (_databaseContextFactory.Create(_idempotenceConnectionStringName))
			{
				_databaseGateway.ExecuteUsing(
					RawQuery.Create(_scriptProvider.GetScript(Script.IdempotenceMessageHandled))
						.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId));
			}
		}
	}
}