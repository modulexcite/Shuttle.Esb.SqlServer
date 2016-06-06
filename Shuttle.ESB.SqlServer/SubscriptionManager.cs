using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.SqlServer
{
	public class SubscriptionManager :
		ISubscriptionManager,
		IRequireInitialization
	{
		private static readonly object Padlock = new object();
		private readonly IDatabaseContextFactory _databaseContextFactory;

		private readonly IDatabaseGateway _databaseGateway;

		private readonly List<string> _deferredSubscriptions = new List<string>();
		private readonly IScriptProvider _scriptProvider;

		private readonly Dictionary<string, List<string>> _subscribers = new Dictionary<string, List<string>>();
		private readonly string _subscriptionConnectionString;

		private IServiceBusConfiguration _serviceBusConfiguration;

		public SubscriptionManager(ISqlServerConfiguration configuration, IScriptProvider scriptProvider,
			IDatabaseContextFactory databaseContextFactory, IDatabaseGateway databaseGateway)
		{
			Guard.AgainstNull(configuration, "configuration");
			Guard.AgainstNull(scriptProvider, "scriptProvider");
			Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
			Guard.AgainstNull(databaseGateway, "databaseGateway");

			_scriptProvider = scriptProvider;
			_databaseContextFactory = databaseContextFactory;
			_databaseGateway = databaseGateway;

			_subscriptionConnectionString = configuration.SubscriptionManagerConnectionString;

			if (string.IsNullOrEmpty(_subscriptionConnectionString))
			{
				throw new ConfigurationErrorsException(string.Format(SqlResources.ConnectionStringEmpty,
					"SubscriptionManager"));
			}
		}

		protected bool HasDeferredSubscriptions
		{
			get { return _deferredSubscriptions.Count > 0; }
		}

		protected bool Started
		{
			get { return _serviceBusConfiguration != null; }
		}

		public void Initialize(IServiceBus bus)
		{
			_serviceBusConfiguration = bus.Configuration;

			using (_databaseContextFactory.Create(SqlServerConfiguration.ProviderName, _subscriptionConnectionString))
			{
				if (_databaseGateway.GetScalarUsing<int>(
					RawQuery.Create(
						_scriptProvider.GetScript(
							Script.SubscriptionManagerExists))) != 1)
				{
				    try
				    {
				        _databaseGateway.ExecuteUsing(RawQuery.Create(
				            _scriptProvider.GetScript(
				                Script.SubscriptionManagerCreate)));
				    }
				    catch (Exception ex)
				    {
				        throw new DataException(SqlResources.SubscriptionManagerCreateException, ex);
				    }
				}
			}

			if (HasDeferredSubscriptions)
			{
				Subscribe(_deferredSubscriptions);
			}
		}

	    public void Subscribe(IEnumerable<string> messageTypeFullNames)
	    {
	        Guard.AgainstNull(messageTypeFullNames, "messageTypeFullNames");

	        if (!Started)
	        {
	            _deferredSubscriptions.AddRange(messageTypeFullNames);

	            return;
	        }

	        if (_serviceBusConfiguration.IsWorker)
	        {
	            return;
	        }

	        if (!_serviceBusConfiguration.HasInbox
	            ||
	            _serviceBusConfiguration.Inbox.WorkQueue == null)
		    {
                throw new SubscriptionManagerException(EsbResources.SubscribeWithNoInboxException);
		    }

		    using (_databaseContextFactory.Create(SqlServerConfiguration.ProviderName, _subscriptionConnectionString))
			{
				foreach (var messageType in messageTypeFullNames)
				{
					_databaseGateway.ExecuteUsing(
						RawQuery.Create(
							_scriptProvider.GetScript(Script.SubscriptionManagerSubscribe))
							.AddParameterValue(SubscriptionManagerColumns.InboxWorkQueueUri,
								_serviceBusConfiguration.Inbox.WorkQueue.Uri.ToString())
							.AddParameterValue(SubscriptionManagerColumns.MessageType, messageType));
				}
			}
		}

		public void Subscribe(string messageTypeFullName)
		{
			Subscribe(new[] {messageTypeFullName});
		}

		public void Subscribe(IEnumerable<Type> messageTypes)
		{
			Subscribe(messageTypes.Select(messageType => messageType.FullName).ToList());
		}

		public void Subscribe(Type messageType)
		{
			Subscribe(new[] {messageType.FullName});
		}

		public void Subscribe<T>()
		{
			Subscribe(new[] {typeof (T).FullName});
		}

		public IEnumerable<string> GetSubscribedUris(object message)
		{
			Guard.AgainstNull(message, "message");

			var messageType = message.GetType().FullName;

			if (!_subscribers.ContainsKey(messageType))
			{
				lock (Padlock)
				{
					if (!_subscribers.ContainsKey(messageType))
					{
						DataTable table;

						using (_databaseContextFactory.Create(SqlServerConfiguration.ProviderName, _subscriptionConnectionString))
						{
							table = _databaseGateway.GetDataTableFor(
								RawQuery.Create(
									_scriptProvider.GetScript(
										Script.SubscriptionManagerInboxWorkQueueUris))
									.AddParameterValue(SubscriptionManagerColumns.MessageType, messageType));
						}

						_subscribers.Add(messageType, (from DataRow row in table.Rows
							select SubscriptionManagerColumns.InboxWorkQueueUri.MapFrom(row))
							.ToList());
					}
				}
			}

			return _subscribers[messageType];
		}

		public static ISubscriptionManager Default()
		{
			var configuration = SqlServerSection.Configuration();

			return
				new SubscriptionManager(configuration,
					new ScriptProvider(configuration.ScriptFolder),
					DatabaseContextFactory.Default(),
					new DatabaseGateway());
		}

		public static ISubscriptionManager Default(ISqlServerConfiguration configuration)
		{
			return
				new SubscriptionManager(configuration,
					new ScriptProvider(configuration.ScriptFolder),
					DatabaseContextFactory.Default(),
					new DatabaseGateway());
		}
	}
}