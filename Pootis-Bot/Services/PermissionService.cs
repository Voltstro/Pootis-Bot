using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Services
{
    public class PermissionService
    {
        readonly CommandService _service;

        private readonly string[] blockedCmds = { "profile", "profilemsg", "hello", "ping", "perm" };

        public PermissionService(CommandService commandService)
        {
            _service = commandService;
        }

        public async Task AddPerm(string command, string role, IMessageChannel channel, SocketGuild guild)
        {
            if(!CanModifyPerm(command))
            {
                await channel.SendMessageAsync($"Cannot set the permission of **{command}**");
                return;
            }

            if (!DoesCmdExist(command))
            {
                await channel.SendMessageAsync($"The command **{command}** doesn't exist!");
                return;
            }

            var server = ServerLists.GetServer(guild);

            //Check too see if role exist
            if(Global.CheckIfRoleExist(guild, role) != null)
            {
                if(server.GetCommandInfo(command) == null) // Command doesn't exist, add it
                {
                    GlobalServerList.CommandInfo item = new GlobalServerList.CommandInfo
                    {
                        Command = command
                    };
                    item.Roles.Add(role);

                    server.CommandInfos.Add(item);
                    
                    await channel.SendMessageAsync($"The role **{role}** was added to the command **{command}**.");
                }
                else // The command already exist, add it to the list of roles.
                {
                    server.GetCommandInfo(command).Roles.Add(role);

                    await channel.SendMessageAsync($"The role **{role}** was added to the command **{command}**.");
                }

                ServerLists.SaveServerList();
            }
            else
            {
                await channel.SendMessageAsync($"The role **{role}** doesn't exist!");
            }
        }

        public async Task RemovePerm(string command, string role, IMessageChannel channel, SocketGuild guild)
        {
            if (!CanModifyPerm(command))
            {
                await channel.SendMessageAsync($"Cannot set the permission of **{command}**");
                return;
            }

            if (!DoesCmdExist(command))
            {
                await channel.SendMessageAsync($"The command **{command}** doesn't exist!");
                return;
            }

            var server = ServerLists.GetServer(guild);

            //Check too see if role exist
            if (Global.CheckIfRoleExist(guild, role) != null)
            {
                if (server.GetCommandInfo(command) == null) // Command already has no permissions
                {
                    await channel.SendMessageAsync($"The command **{command}** already has no permissions added to it!");
                    return;
                }
                else // Remove the role
                {
                    bool roleRemoved = false;
                    foreach(var _role in server.GetCommandInfo(command).Roles.ToArray())
                    {
                        if (roleRemoved)
                            continue;

                        if(role == _role)
                        {
                            server.GetCommandInfo(command).Roles.Remove(role);
                            roleRemoved = true;

                            await channel.SendMessageAsync($"The role **{role}** was removed from the command **{command}**.");
                        }
                    }

                    if (!roleRemoved)
                        await channel.SendMessageAsync($"The command **{command}** didn't had the role **{role}** on it!");

                    if(server.GetCommandInfo(command).Roles.Count == 0)
                    {
                        server.CommandInfos.Remove(server.GetCommandInfo(command));
                    }

                }

                ServerLists.SaveServerList();
            }
            else
            {
                await channel.SendMessageAsync($"The role **{role}** doesn't exist!");
            }
        }

        private bool CanModifyPerm(string command)
        {
            bool isModifyable = true;
            foreach(string cmd in blockedCmds)
            {
                if(command == cmd)
                {
                    isModifyable = false;
                }
            }

            return isModifyable;
        }

        private bool DoesCmdExist(string command)
        {
            CommandInfo cmdinfo;
            bool stopsearch = false;

            foreach (var moduel in _service.Modules) //Get the command info
            {
                if (stopsearch)
                    continue;

                foreach (var _command in moduel.Commands)
                {
                    if (_command.Name == command)
                    {
                        cmdinfo = _command;
                        stopsearch = true;
                    }
                }
            }

            if (!stopsearch)
            {
                return false;
            }

            return true;
        }
    }
}
