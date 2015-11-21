using System;
using System.IO;

namespace Shuttle.ESB.SqlServer.Tests
{
    public class SqlServerSectionFixture
    {
        protected SqlServerSection GetSqlServerSection(string file)
        {
            return SqlServerSection.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@".\SqlServerSection\files\{0}", file)));
        }
    }
}