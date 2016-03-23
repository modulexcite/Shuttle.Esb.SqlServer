using System;

namespace Shuttle.Esb.SqlServer
{
    public class ScriptException : Exception
    {
        public ScriptException(string message)
            : base(message)
        {
        }
    }
}