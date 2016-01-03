using System;
using System.IO;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.SqlServer.Tests
{
	public class SqlServerSectionFixture
	{
		protected SqlServerSection GetSqlServerSection(string file)
		{
			return ConfigurationSectionProvider.OpenFile<SqlServerSection>("shuttle", "sqlServer", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@".\SqlServerSection\files\{0}", file)));
		}
	}
}