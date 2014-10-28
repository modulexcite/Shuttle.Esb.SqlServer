using NUnit.Framework;
using Shuttle.ESB.Tests;

namespace Shuttle.ESB.SqlServer.Tests
{
	public class SqlDeferredMessageTest : DeferredFixture
	{
		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_be_able_to_perform_full_processing(bool isTransactionalEndpoint)
		{
			TestDeferredProcessing("sql://shuttle/{0}", isTransactionalEndpoint);
		}
	}
}