using System;
using System.IO;

namespace Shuttle.Esb.SqlServer
{
	public class SqlServerConfiguration : ISqlServerConfiguration
	{
        internal static string ProviderName = "System.Data.SqlClient";

        private string _scriptFolder;

		public SqlServerConfiguration()
		{
			ScriptFolder = null;
		}

		public string SubscriptionManagerConnectionString { get; set; }
		public string IdempotenceServiceConnectionString { get; set; }

		public string ScriptFolder
		{
			get { return _scriptFolder; }
			set
			{
				_scriptFolder = value;

				if (string.IsNullOrEmpty(_scriptFolder))
				{
					_scriptFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts");
				}
				else
				{
					if (!Path.IsPathRooted(ScriptFolder))
					{
						_scriptFolder = Path.GetFullPath(ScriptFolder);
					}
				}
			}
		}
	}
}