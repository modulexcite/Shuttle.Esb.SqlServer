using System.Configuration;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb;

namespace Shuttle.Esb.SqlServer
{
    public class SqlServerSection : ConfigurationSection
    {
        [ConfigurationProperty("subscriptionManagerConnectionStringName", IsRequired = false,
            DefaultValue = "Subscription")]
        public string SubscriptionManagerConnectionStringName
        {
            get { return (string) this["subscriptionManagerConnectionStringName"]; }
        }

        [ConfigurationProperty("idempotenceServiceConnectionStringName", IsRequired = false,
            DefaultValue = "Idempotence")]
        public string IdempotenceServiceConnectionStringName
        {
            get { return (string) this["idempotenceServiceConnectionStringName"]; }
        }

        [ConfigurationProperty("scriptFolder", IsRequired = false, DefaultValue = null)]
        public string ScriptFolder
        {
            get { return (string) this["scriptFolder"]; }
        }

        public static SqlServerConfiguration Configuration()
        {
            var section = ConfigurationSectionProvider.Open<SqlServerSection>("shuttle", "sqlServer");
            var configuration = new SqlServerConfiguration();

            var subscriptionManagerConnectionStringName = "Subscription";
            var idempotenceServiceConnectionStringName = "Idempotence";

            if (section != null)
            {
                subscriptionManagerConnectionStringName = section.SubscriptionManagerConnectionStringName;
                idempotenceServiceConnectionStringName = section.IdempotenceServiceConnectionStringName;
                configuration.ScriptFolder = section.ScriptFolder;
            }

            configuration.SubscriptionManagerConnectionString = GetConnectionString(subscriptionManagerConnectionStringName);
            configuration.IdempotenceServiceConnectionString = GetConnectionString(idempotenceServiceConnectionStringName);

            return configuration;
        }

        private static string GetConnectionString(string connectionStringName)
        {
            var settings = ConfigurationManager.ConnectionStrings[connectionStringName];

            return settings == null ? string.Empty : settings.ConnectionString;
        }
    }
}