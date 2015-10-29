using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.ESB.SqlServer
{
	public class ScriptProvider : IScriptProvider
	{
		private readonly string _scriptFolder;
		private static readonly object Padlock = new object();

		private readonly Dictionary<Script, string> _scripts = new Dictionary<Script, string>();

		public ScriptProvider(string scriptFolder)
		{
            Guard.AgainstNullOrEmptyString(scriptFolder, "scriptFolder");

			_scriptFolder = scriptFolder;
		}

		public string GetScript(Script script, params string[] parameters)
		{
			if (!_scripts.ContainsKey(script))
			{
				AddScript(script);
			}

			return parameters != null
					   ? string.Format(_scripts[script], parameters)
					   : _scripts[script];
		}

		private void AddScript(Script script)
		{
			lock (Padlock)
			{
				if (_scripts.ContainsKey(script))
				{
					return;
				}

				var files = new string[0];

				if (Directory.Exists(_scriptFolder))
				{
					files = Directory.GetFiles(_scriptFolder, script.FileName, SearchOption.AllDirectories);
				}

				if (files.Length == 0)
				{
					AddEmbeddedScript(script);

					return;
				}

				if (files.Length > 1)
				{
					throw new ScriptException(string.Format(SqlResources.ScriptCountException, _scriptFolder, script.FileName, files.Length));
				}

				_scripts.Add(script, File.ReadAllText(files[0]));
			}
		}

		private void AddEmbeddedScript(Script script)
		{
			using (var stream =
				Assembly.GetCallingAssembly().GetManifestResourceStream(
					string.Format("Shuttle.ESB.SqlServer.Scripts.{0}", script.FileName)))
			{
				if (stream == null)
				{
					throw new ScriptException(string.Format(SqlResources.EmbeddedScriptMissingException, script.FileName));
				}

				using (var reader = new StreamReader(stream))
				{
					_scripts.Add(script, reader.ReadToEnd());
				}
			}
		}

		public static IScriptProvider Default()
		{
			return new ScriptProvider(SqlServerSection.Configuration().ScriptFolder);
		}
	}
}