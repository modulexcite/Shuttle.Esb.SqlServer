namespace Shuttle.ESB.SqlServer.Tests
{
	public class SqlServerSectionFixture
	{
		protected SqlServerSection GetSqlServerSection(string file)
		{
			return SqlServerSection.Open(string.Format(@".\SqlServerSection\files\{0}", file));
		}
	}
}