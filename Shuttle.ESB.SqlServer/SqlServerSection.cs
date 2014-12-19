using System.Configuration;
using Shuttle.ESB.Core;

namespace Shuttle.ESB.SqlServer
{
	public class SqlServerSection : ConfigurationSection
	{
		public static SqlServerSection Open(string file)
		{
			return ShuttleConfigurationSection.Open<SqlServerSection>("sqlServer", file);
		}

		[ConfigurationProperty("subscriptionManagerConnectionStringName", IsRequired = false, DefaultValue = "Subscription")]
		public string SubscriptionManagerConnectionStringName
		{
			get
			{
				return (string)this["subscriptionManagerConnectionStringName"];
			}
		}

		[ConfigurationProperty("idempotenceServiceConnectionStringName", IsRequired = false, DefaultValue = "Idempotence")]
		public string IdempotenceServiceConnectionStringName
		{
			get
			{
				return (string)this["idempotenceServiceConnectionStringName"];
			}
		}

		[ConfigurationProperty("scriptFolder", IsRequired = false, DefaultValue = null)]
		public string ScriptFolder
		{
			get
			{
				return (string)this["scriptFolder"];
			}
		}

		public static SqlServerConfiguration Configuration()
		{
			var section = ShuttleConfigurationSection.Open<SqlServerSection>("sqlServer");
			var configuration = new SqlServerConfiguration();

			if (section != null)
			{
				configuration.SubscriptionManagerConnectionStringName = section.SubscriptionManagerConnectionStringName;
				configuration.IdempotenceServiceConnectionStringName = section.IdempotenceServiceConnectionStringName;
				configuration.ScriptFolder = section.ScriptFolder;
			}

			return configuration;
		}
	}
}