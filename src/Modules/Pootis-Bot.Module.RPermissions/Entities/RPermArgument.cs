namespace Pootis_Bot.Module.RPermissions.Entities;

public struct RPermArgument
{
    internal RPermArgument(string argumentName, string argumentType)
    {
        ArgumentName = argumentName;
        ArgumentType = argumentType;
    }
    
    public string ArgumentName { get; }
    public string ArgumentType { get; }
}