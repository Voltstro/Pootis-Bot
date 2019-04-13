using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pootis_Bot.Core
{
    public class CommandHandler
    {
        //https://docs.stillu.cc/guides/commands/intro.html
        //For helping me updated the old commandhandler to discord.net 2.0

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public CommandHandler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }    

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage msg)) return;
            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            if (msg.Author.IsBot) //Check to see if user is bot, if is bot return.
                return;
            LevelingSystem.UserSentMessage((SocketGuildUser)context.User, (SocketTextChannel)context.Channel, 10);

            foreach (var item in ServerLists.GetServer(context.Guild).GetAllBanedChannels())//Check to channel, make sure its not on the baned list
            {
                if (msg.Channel.Id == item.channelID)
                    return;
            }
            if (msg.HasStringPrefix(Global.botPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                //Permissions
                var cmdSearchResult = _commands.Search(context, argPos);
                if (!cmdSearchResult.IsSuccess) return;

                var perm = ServerLists.GetServer(context.Guild).GetCommandInfo(cmdSearchResult.Commands[0].Command.Name);
                if(perm.Command != null)
                {
                    var _role = (context.User as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == perm.Role);
                    var user = (SocketGuildUser)context.User;

                    if (!user.Roles.Contains(_role))
                        return;
                }

                var result = await _commands.ExecuteAsync(context, argPos, services: null);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Global.WriteMessage(result.ErrorReason, ConsoleColor.Red);
                }
            }
        }
    }
}
