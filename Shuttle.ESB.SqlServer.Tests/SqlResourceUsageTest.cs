using NUnit.Framework;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.SqlServer.Tests
{
	public class SqlResourceUsageTest : ResourceUsageFixture
	{
		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_not_exceeed_normal_resource_usage(bool isTransactionalEndpoint)
		{
			TestResourceUsage("sql://shuttle/{0}", isTransactionalEndpoint);
		}
	}
}