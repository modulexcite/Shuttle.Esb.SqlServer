namespace Shuttle.ESB.SqlServer
{
	public interface ISqlServerConfiguration
	{
		string SubscriptionManagerConnectionString { get; }
		string IdempotenceServiceConnectionString { get; }
		string ScriptFolder { get; }
	}
}