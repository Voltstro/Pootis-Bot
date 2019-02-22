using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;
using System.Threading.Tasks;

namespace Pootis_Bot.Services
{
    public class PermissionService
    {
        CommandService _service;

        public PermissionService(CommandService commandService)
        {
            _service = commandService;
        }

        public async Task SetPermission(string _command, string role, IMessageChannel channel, SocketGuild guild)
        {
            if (_command == "profile" || _command == "profilemsg")
            {
                await channel.SendMessageAsync($"Cannot set the permission of {_command}");
                return;
            }

            CommandInfo cmdinfo = null;
            bool stopsearch = false;

            foreach (var moduel in _service.Modules)
            {
                if (stopsearch)
                    continue;

                foreach (var command in moduel.Commands)
                {
                    if (command.Name == _command)
                    {
                        cmdinfo = command;
                        stopsearch = true;
                        continue;
                    }
                }
            }

            if (!stopsearch)
            {
                await channel.SendMessageAsync($"Cannot find the command {_command}");
                return;
            }

            var server = ServerLists.GetServer(guild);

            if (Global.CheckIfRoleExist(guild, role) == null)
            {
                if (role == "remove")
                {
                    if (RemovePermission(_command, server, channel) == true)
                    {
                        return;
                    }
                    else
                    {
                        await channel.SendMessageAsync("That command alreadys has no permission");
                        return;
                    }
                }
                else
                {
                    await channel.SendMessageAsync("That role doesn't exist");
                }
            }
            
            if (server.GetCommandInfo(_command).Command == null)
            {
                GlobalServerList.CommandInfo item = new GlobalServerList.CommandInfo()
                {
                    Command = _command,
                    Role = role
                };
                server.commandInfos.Add(item);
                ServerLists.SaveServerList();

                await channel.SendMessageAsync($"The command '{_command}' had it permission set the role '{role}'");
            }
            else
            {
                var command = server.GetCommandInfo(_command);
                command.Role = role;
                ServerLists.SaveServerList();
                await channel.SendMessageAsync($"The command '{_command}' had it permission set the role '{role}'");

            }
        }

        private bool RemovePermission(string command, GlobalServerList _server, IMessageChannel _channel)
        {
            var commandInfo = _server.GetCommandInfo(command);
            if(commandInfo.Command == null)
                return false;
            else
            {
                _server.commandInfos.Remove(commandInfo);
                ServerLists.SaveServerList();
                _channel.SendMessageAsync($"Permmsions for {command} was removed");

                return true;
            }
        }
    }
}
