namespace Shuttle.Esb.SqlServer
{
    public interface IScriptProvider
    {
        string GetScript(Script script, params string[] parameters);
    }
}