using System;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.ESB.Core;

namespace Shuttle.ESB.SqlServer
{
	public class SqlQueueFactory : IQueueFactory
	{
		private readonly IScriptProvider _scriptProvider;
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IDatabaseGateway _databaseGateway;

		public SqlQueueFactory()
		{
			_scriptProvider = ScriptProvider.Default();
			_databaseConnectionFactory = DatabaseConnectionFactory.Default();
			_databaseGateway = DatabaseGateway.Default();
		}

		public SqlQueueFactory(IScriptProvider scriptProvider, IDatabaseConnectionFactory databaseConnectionFactory, IDatabaseGateway databaseGateway)
		{
			_scriptProvider = scriptProvider;
			_databaseConnectionFactory = databaseConnectionFactory;
			_databaseGateway = databaseGateway;
		}

		public string Scheme
		{
			get { return SqlUriParser.SCHEME; }
		}

		public IQueue Create(Uri uri)
		{
			Guard.AgainstNull(uri, "uri");

			return new SqlQueue(uri, _scriptProvider, _databaseConnectionFactory, _databaseGateway);
		}

		public bool CanCreate(Uri uri)
		{
			Guard.AgainstNull(uri, "uri");

			return Scheme.Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}