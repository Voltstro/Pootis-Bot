using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Pootis_Bot.Core
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public CommandHandler(DiscordSocketClient client)
        {
            _commands = new CommandService();
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

            foreach (var item in ServerLists.GetServer(context.Guild).BanedChannels) //Check to channel, make sure its not on the baned list
            {
                if (msg.Channel.Id == item)
                    return;
            }
            if (msg.HasStringPrefix(Global.botPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                //Permissions
                var cmdSearchResult = _commands.Search(context, argPos);
                if (!cmdSearchResult.IsSuccess) return;

                var perm = ServerLists.GetServer(context.Guild).GetCommandInfo(cmdSearchResult.Commands[0].Command.Name);
                if(perm != null)
                {
                    bool doesUserHaveARole = false;
                    
                    foreach (var role in perm.Roles)
                    {
                        var _role = (context.User as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == role);
                        if ((context.User as SocketGuildUser).Roles.Contains(_role))
                        {
                            doesUserHaveARole = true;
                        }
                    }

                    if (!doesUserHaveARole && context.User != context.Guild.Owner)
                        return;
                }

                var result = await _commands.ExecuteAsync(context, argPos, services: null);
				if (!result.IsSuccess && result.Error == CommandError.BadArgCount)
					await context.Channel.SendMessageAsync($"The command `{msg.Content.Replace(Global.botPrefix, "")}` either has too many or too little arguments!");
				else if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
					Global.Log(result.ErrorReason, ConsoleColor.Red);
            }
            else
            {
                var account = UserAccounts.GetAccount((SocketGuildUser)context.User).GetOrCreateServer(context.Guild.Id);
                DateTime now = DateTime.Now;
                
                //Only level it up if the last message was the level up cooldown.
                if(account.LastLevelUpTime.Subtract(now).TotalSeconds == Config.bot.levelUpCooldown || account.LastLevelUpTime.Second == 0)
                {
                    LevelingSystem.UserSentMessage((SocketGuildUser)context.User, (SocketTextChannel)context.Channel, 10);

                    //We dont need to save the accounts file
                    account.LastLevelUpTime = now;
                }
            }
        }
    }
}
