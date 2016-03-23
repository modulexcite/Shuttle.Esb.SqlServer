using NUnit.Framework;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.SqlServer.Tests
{
	public class SqlOutboxTest : OutboxFixture
	{
		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_be_able_handle_errors(bool isTransactionalEndpoint)
		{
			TestOutboxSending("sql://shuttle/{0}", isTransactionalEndpoint);
		}
	}
}