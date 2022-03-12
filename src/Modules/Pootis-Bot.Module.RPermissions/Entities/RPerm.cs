using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pootis_Bot.Module.RPermissions.Entities;

public class RPerm
{
    internal RPerm(string command)
    {
        Command = command;
        Arguments = new List<RPermArgument>();
        Roles = new List<ulong>();
    }

    [JsonConstructor]
    internal RPerm(string command, List<RPermArgument> arguments, List<ulong> roles)
    {
        Command = command;
        Arguments = arguments;
        Roles = roles;
    }
    
    public string Command { get; }

    public List<RPermArgument> Arguments { get; }

    public List<ulong> Roles { get; }

    /// <summary>
    ///     Adds a role
    /// </summary>
    /// <param name="role"></param>
    public void AddRole(ulong role)
    {
        Roles.Add(role);
    }

    /// <summary>
    ///     Does a role exist?
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public bool DoesRoleExist(ulong role)
    {
        return Roles.Contains(role);
    }
}