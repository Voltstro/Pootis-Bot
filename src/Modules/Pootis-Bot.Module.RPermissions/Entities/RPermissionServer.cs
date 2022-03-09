using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord.Commands;
using Discord.Interactions;

namespace Pootis_Bot.Module.RPermissions.Entities
{
    public class RPermissionServer
    {
        public ulong GuildId { get; set; }

        public List<RPerm> Permissions { get; set; } = new List<RPerm>();

        public List<RPerm> SlashCommandPermissions { get; } = new List<RPerm>();

        [return: MaybeNull]
        public RPerm GetPermissionForCommand(CommandInfo command)
        {
            //Get all permissions for a command
            RPerm[] perms = Permissions.Where(x => x.Command == command.Name).ToArray();
            if (perms.Length == 0)
                return null;

            RPerm selectedRPerm = null;
            foreach (RPerm perm in perms)
            {
                if (command.Parameters.Any(parameter => !perm.Arguments.Any(x =>
                    x.ArgumentName == parameter.Name && x.ArgumentType == parameter.Type.FullName)))
                {
                    continue;
                }

                selectedRPerm = perm;
            }

            return selectedRPerm;
        }

        public RPerm AddCommandPermission(CommandInfo command)
        {
            RPerm perm = new RPerm
            {
                Command = command.Name,
                Roles = new List<ulong>()
            };
            foreach (ParameterInfo parameter in command.Parameters)
            {
                perm.Arguments.Add(new RPermArgument
                {
                    ArgumentName = parameter.Name,
                    ArgumentType = parameter.Type.FullName
                });
            }
            Permissions.Add(perm);
            return perm;
        }

        [return: MaybeNull]
        public RPerm SlashCommandGetPerm(SlashCommandInfo command)
        {
            //Get all permissions for a command
            RPerm[] perms = SlashCommandPermissions.Where(x => x.Command == command.Name).ToArray();
            if (perms.Length == 0)
                return null;

            RPerm selectedRPerm = null;
            foreach (RPerm perm in perms)
            {
                if (command.Parameters.Any(parameter => !perm.Arguments.Any(x =>
                        x.ArgumentName == parameter.Name && x.ArgumentType == parameter.ParameterType.FullName)))
                {
                    continue;
                }

                selectedRPerm = perm;
            }

            return selectedRPerm;
        }

        public RPerm SlashCommandAddPerm(SlashCommandInfo command)
        {
            RPerm perm = new RPerm
            {
                Command = command.Name,
                Roles = new List<ulong>()
            };
            foreach (SlashCommandParameterInfo parameter in command.Parameters)
            {
                perm.Arguments.Add(new RPermArgument
                {
                    ArgumentName = parameter.Name,
                    ArgumentType = parameter.ParameterType.FullName
                });
            }
            SlashCommandPermissions.Add(perm);
            return perm;
        }
    }
}