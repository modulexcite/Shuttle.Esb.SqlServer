using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.ESB.Core;

namespace Shuttle.ESB.SqlServer.Idempotence
{
	public class IdempotenceService :
		IIdempotenceService,
		IRequireInitialization
	{
		private readonly DataSource _idempotenceDataSource ;

		private readonly IDatabaseGateway _databaseGateway;
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IScriptProvider _scriptProvider;

		public static IIdempotenceService Default()
		{
			var configuration = SqlServerSection.Configuration();

			return
				new IdempotenceService(configuration,
					new ScriptProvider(configuration),
					DatabaseConnectionFactory.Default(),
					DatabaseGateway.Default());
		}


		public IdempotenceService(
			ISqlServerConfiguration configuration,
			IScriptProvider scriptProvider,
			IDatabaseConnectionFactory databaseConnectionFactory,
			IDatabaseGateway databaseGateway)
		{
			Guard.AgainstNull(configuration, "configuration");
			Guard.AgainstNull(scriptProvider, "_scriptProvider");
			Guard.AgainstNull(databaseConnectionFactory, "_databaseConnectionFactory");
			Guard.AgainstNull(databaseGateway, "_databaseGateway");

			_scriptProvider = scriptProvider;
			_databaseConnectionFactory = databaseConnectionFactory;
			_databaseGateway = databaseGateway;

			_idempotenceDataSource = new DataSource(configuration.IdempotenceServiceConnectionStringName, new SqlDbDataParameterFactory());
		}

		public void Initialize(IServiceBus bus)
		{
			using (var connection = _databaseConnectionFactory.Create(_idempotenceDataSource))
			using (var transaction = connection.BeginTransaction())
			{
				if (_databaseGateway.GetScalarUsing<int>(
					_idempotenceDataSource,
					RawQuery.Create(
						_scriptProvider.GetScript(
							Script.IdempotenceServiceExists))) != 1)
				{
					throw new IdempotenceServiceException(SqlResources.IdempotenceDatabaseNotConfigured);
				}

				_databaseGateway.ExecuteUsing(
					_idempotenceDataSource,
					RawQuery.Create(
						_scriptProvider.GetScript(
							Script.IdempotenceInitialize))
						.AddParameterValue(IdempotenceColumns.InboxWorkQueueUri, bus.Configuration.Inbox.WorkQueue.Uri.ToString()));

				transaction.CommitTransaction();
			}
		}

		public bool ShouldProcess(TransportMessage transportMessage)
		{
			try
			{
				using (var connection = _databaseConnectionFactory.Create(_idempotenceDataSource))
				{
					if (_databaseGateway.GetScalarUsing<int>(
						_idempotenceDataSource,
						RawQuery.Create(
							_scriptProvider.GetScript(
								Script.IdempotenceHasCompleted))
							.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)) == 1)
					{
						return false;
					}

					if (_databaseGateway.GetScalarUsing<int>(
						_idempotenceDataSource,
						RawQuery.Create(
							_scriptProvider.GetScript(
								Script.IdempotenceIsProcessing))
							.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)) == 1)
					{
						return false;
					}

					using (var transaction = connection.BeginTransaction())
					{
						_databaseGateway.ExecuteUsing(
							_idempotenceDataSource,
							RawQuery.Create(
								_scriptProvider.GetScript(
									Script.IdempotenceProcessing))
								.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)
								.AddParameterValue(IdempotenceColumns.InboxWorkQueueUri, transportMessage.RecipientInboxWorkQueueUri));

						transaction.CommitTransaction();
					}
				}

				return true;
			}
			catch (SqlException ex)
			{
				var message = ex.Message.ToUpperInvariant();

				if (message.Contains("VIOLATION OF UNIQUE KEY CONSTRAINT") || message.Contains("CANNOT INSERT DUPLICATE KEY"))
				{
					return false;
				}

				throw;
			}
		}

		public void ProcessingCompleted(TransportMessage transportMessage)
		{
			using (var connection = _databaseConnectionFactory.Create(_idempotenceDataSource))
			using (var transaction = connection.BeginTransaction())
			{
				_databaseGateway.ExecuteUsing(
					_idempotenceDataSource,
					RawQuery.Create(
						_scriptProvider.GetScript(Script.IdempotenceComplete))
						.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId));

				transaction.CommitTransaction();
			}
		}

		public void AddDeferredMessage(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage,
			Stream deferredTransportMessageStream)
		{
			using (_databaseConnectionFactory.Create(_idempotenceDataSource))
			{
				_databaseGateway.ExecuteUsing(
					_idempotenceDataSource,
					RawQuery.Create(_scriptProvider.GetScript(Script.IdempotenceSendDeferredMessage))
						.AddParameterValue(IdempotenceColumns.MessageId, deferredTransportMessage.MessageId)
						.AddParameterValue(IdempotenceColumns.MessageIdReceived, processingTransportMessage.MessageId)
						.AddParameterValue(IdempotenceColumns.MessageBody, deferredTransportMessageStream.ToBytes()));
			}
		}

		public IEnumerable<Stream> GetDeferredMessages(TransportMessage transportMessage)
		{
			var result = new List<Stream>();

			using (_databaseConnectionFactory.Create(_idempotenceDataSource))
			{
				var rows = _databaseGateway.GetRowsUsing(
					_idempotenceDataSource,
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
			using (_databaseConnectionFactory.Create(_idempotenceDataSource))
			{
				_databaseGateway.ExecuteUsing(
					_idempotenceDataSource,
					RawQuery.Create(_scriptProvider.GetScript(Script.IdempotenceDeferredMessageSent))
						.AddParameterValue(IdempotenceColumns.MessageId, deferredTransportMessage.MessageId));
			}
		}
	}
}