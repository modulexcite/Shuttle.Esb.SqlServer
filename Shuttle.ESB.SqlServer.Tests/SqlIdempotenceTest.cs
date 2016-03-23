using NUnit.Framework;
using Shuttle.Esb.SqlServer.Idempotence;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.SqlServer.Tests
{
	[TestFixture]
	public class SqlIdempotenceTest : IdempotenceFixture
	{
		[Test]
		[TestCase(false, false)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(true, true)]
		public void Should_be_able_to_perform_full_processing(bool isTransactionalEndpoint, bool enqueueUniqueMessages)
		{
			TestIdempotenceProcessing(IdempotenceService.Default(), @"sql://shuttle/{0}", isTransactionalEndpoint, enqueueUniqueMessages);
		}
	}
}