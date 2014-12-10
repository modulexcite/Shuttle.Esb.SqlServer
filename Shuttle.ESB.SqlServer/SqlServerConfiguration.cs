using System;
using System.IO;

namespace Shuttle.ESB.SqlServer
{
	public class SqlServerConfiguration : ISqlServerConfiguration
	{
		private string _scriptFolder;

		public SqlServerConfiguration()
		{
			SubscriptionManagerConnectionStringName = "Subscription";
			IdempotenceServiceConnectionStringName = "Idempotence";
			ScriptFolder = null;
		}

		public string SubscriptionManagerConnectionStringName { get; set; }
		public string IdempotenceServiceConnectionStringName { get; set; }

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