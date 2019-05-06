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
        readonly CommandService _service;

        public PermissionService(CommandService commandService)
        {
            _service = commandService;
        }

        public async Task SetPermission(string _command, string role, IMessageChannel channel, SocketGuild guild)
        {
            if (_command == "profile" || _command == "profilemsg") //Check to see if the inputed command isn't invaild
            {
                await channel.SendMessageAsync($"Cannot set the permission of {_command}");
                return;
            }

            CommandInfo cmdinfo;
            bool stopsearch = false;

            foreach (var moduel in _service.Modules) //Get the command info
            {
                if (stopsearch)
                    continue;

                foreach (var command in moduel.Commands)
                {
                    if (command.Name == _command)
                    {
                        cmdinfo = command;
                        stopsearch = true;
                    }
                }
            }

            if (!stopsearch)
            {
                await channel.SendMessageAsync($"Cannot find the command {_command}");
                return;
            }

            var server = ServerLists.GetServer(guild);

            if (Global.CheckIfRoleExist(guild, role) == null) //Check to see if the inputed role exist
            {
                if (role == "remove") //If the role = remove then remove the permission
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
                GlobalServerList.CommandInfo item = new GlobalServerList.CommandInfo
                {
                    Command = _command,
                    Role = role
                };
                server.commandInfos.Add(item); //Create and set the permission
                ServerLists.SaveServerList();

                await channel.SendMessageAsync($"The command '**{_command}**' had its permission set to the role '**{role}**'");
            }
            else
            {
                var command = server.GetCommandInfo(_command);
                command.Role = role;    //Set the permission
                ServerLists.SaveServerList();
                await channel.SendMessageAsync($"The command '**{_command}**' had it permission set to the role '**{role}**'");
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
