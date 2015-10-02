using System;
using System.Data;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.SqlServer
{
	public class SqlQueueDatabaseConnectionFactory : IDatabaseConnectionFactory
	{
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IDatabaseConnectionCache _databaseConnectionCache;
		private readonly IDbCommandFactory _dbCommandFactory;

		[ThreadStatic]
		private static IDbConnection _dbConnection;

		public SqlQueueDatabaseConnectionFactory(IDbConnectionFactory dbConnectionFactory, IDbCommandFactory dbCommandFactory,
			IDatabaseConnectionCache databaseConnectionCache)
		{
			Guard.AgainstNull(dbConnectionFactory, "dbConnectionFactory");
			Guard.AgainstNull(dbCommandFactory, "dbCommandFactory");
			Guard.AgainstNull(databaseConnectionCache, "databaseConnectionCache");

			_databaseConnectionFactory = new DatabaseConnectionFactory(dbConnectionFactory, dbCommandFactory,
				databaseConnectionCache);
			_databaseConnectionCache = databaseConnectionCache;
			_dbCommandFactory = dbCommandFactory;
		}

		public static IDatabaseConnectionFactory Default()
		{
			return new SqlQueueDatabaseConnectionFactory(DbConnectionFactory.Default(), new DbCommandFactory(),
				new ThreadStaticDatabaseConnectionCache());
		}

		public void AssignThreadDbConnection(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		public IDatabaseConnection Create(DataSource dataSource)
		{
			return _dbConnection != null
				? new DatabaseConnection(dataSource, _dbConnection, _dbCommandFactory, _databaseConnectionCache)
				: _databaseConnectionFactory.Create(dataSource);
		}
	}
}