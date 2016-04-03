using System;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.SqlServer
{
	public class SqlQueueFactory : IQueueFactory
	{
		private readonly IScriptProvider _scriptProvider;
		private readonly IDatabaseContextFactory _databaseContextFactory;
		private readonly IDatabaseGateway _databaseGateway;

		public SqlQueueFactory()
		{
			_scriptProvider = ScriptProvider.Default();
			_databaseContextFactory = DatabaseContextFactory.Default();
			_databaseGateway = new DatabaseGateway();
		}

		public SqlQueueFactory(IScriptProvider scriptProvider, IDatabaseContextFactory databaseContextFactory,
			IDatabaseGateway databaseGateway)
		{
			_scriptProvider = scriptProvider;
			_databaseContextFactory = databaseContextFactory;
			_databaseGateway = databaseGateway;
		}

		public string Scheme
		{
			get { return SqlUriParser.SCHEME; }
		}

		public IQueue Create(Uri uri)
		{
			Guard.AgainstNull(uri, "uri");

			return new SqlQueue(uri, _scriptProvider, _databaseContextFactory, _databaseGateway);
		}

		public bool CanCreate(Uri uri)
		{
			Guard.AgainstNull(uri, "uri");

			return Scheme.Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}