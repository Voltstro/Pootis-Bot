using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using Discord.Interactions;

namespace Pootis_Bot.Module.RPermissions.Entities;

/// <summary>
///     Permissions for a guild
/// </summary>
public class RPermissionServer
{
    public RPermissionServer(ulong guildId)
    {
        GuildId = guildId;
        SlashCommandPermissions = new List<RPerm>();
    }
    
    public ulong GuildId { get; }
    public List<RPerm> SlashCommandPermissions { get; }

    /// <summary>
    ///     Gets a <see cref="RPerm"/> for a <see cref="SlashCommandInfo"/>
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public RPerm? SlashCommandGetPerm(SlashCommandInfo command)
    {
        //Get all permissions for a command
        RPerm[] perms = SlashCommandPermissions.Where(x => x.Command == command.Name).ToArray();
        if (perms.Length == 0)
            return null;

        RPerm? selectedRPerm = null;
        foreach (RPerm perm in perms)
        {
            if (command.Parameters.Any(parameter => !perm.Arguments.Any(x =>
                    x.ArgumentName == parameter.Name && x.ArgumentType == parameter.ParameterType.FullName)))
                continue;

            selectedRPerm = perm;
        }

        return selectedRPerm;
    }

    /// <summary>
    ///     Adds a <see cref="RPerm"/> for a <see cref="SlashCommandInfo"/>
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public RPerm SlashCommandAddPerm(SlashCommandInfo command)
    {
        RPerm perm = new(command.Name);
        foreach (SlashCommandParameterInfo parameter in command.Parameters)
        {
            string? parameterType = parameter.ParameterType.FullName;
            if (parameterType == null)
                throw new NullReferenceException("A parameter type's full name was null!");
            
            perm.Arguments.Add(new RPermArgument(parameter.Name, parameterType));
        }
        SlashCommandPermissions.Add(perm);
        return perm;
    }
}