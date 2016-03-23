namespace Shuttle.Esb.SqlServer
{
	public interface ISqlServerConfiguration
	{
		string SubscriptionManagerConnectionString { get; }
		string IdempotenceServiceConnectionString { get; }
		string ScriptFolder { get; }
	}
}