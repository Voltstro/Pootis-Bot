using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Entities;
using Pootis_Bot.Services.AntiSpam;

namespace Pootis_Bot.Core
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
		private readonly AntiSpamService _antiSpam;

        public CommandHandler(DiscordSocketClient client)
        {
            _commands = new CommandService();
			_antiSpam = new AntiSpamService();
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
            SocketCommandContext context = new SocketCommandContext(_client, msg);
            GlobalServerList server = ServerLists.GetServer(context.Guild);
            int argPos = 0;

			if (msg.Author.IsBot) //Check to see if user is bot, if is bot return.
                return;

			//Someone has mention more than 2 users, check with the anti-spam
			if (msg.MentionedUsers.Count >= 2)
			{
				if (_antiSpam.CheckMentionUsers(msg, context.Guild))
					return;
			}

			//There are role mentions
			if(msg.MentionedRoles.Count >= 1)
			{
				if (_antiSpam.CheckRoleMentions(msg, (SocketGuildUser)msg.Author))
					return;
			}

			foreach (var item in server.BannedChannels) //Check to channel, make sure its not on the baned list
            {
                if (msg.Channel.Id == item)
                    return;
            }
            if (msg.HasStringPrefix(Global.BotPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                //Permissions
                var cmdSearchResult = _commands.Search(context, argPos);
                if (!cmdSearchResult.IsSuccess) return;

                var perm = server.GetCommandInfo(cmdSearchResult.Commands[0].Command.Name);
                if(perm != null)
                {
                    bool doesUserHaveARole = false;
                    
                    foreach (var role in perm.Roles)
                    {
                        var guildRole = (context.User as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == role);
                        if ((context.User as SocketGuildUser).Roles.Contains(guildRole))
                        {
                            doesUserHaveARole = true;
                        }
                    }

                    if (!doesUserHaveARole && context.User.Id != context.Guild.Owner.Id)
                        return;
                }

                var result = await _commands.ExecuteAsync(context, argPos, services: null);
				if (!result.IsSuccess && result.Error == CommandError.BadArgCount)
					await context.Channel.SendMessageAsync($"The command `{msg.Content.Replace(Global.BotPrefix, "")}` either has too many or too little arguments!");
				else if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
					await context.Channel.SendMessageAsync(result.ErrorReason);
				else if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
					Global.Log(result.ErrorReason, ConsoleColor.Red);
            }
            else
            {
                var account = UserAccounts.GetAccount((SocketGuildUser)context.User).GetOrCreateServer(context.Guild.Id);
                DateTime now = DateTime.Now;
                
                //Only level it up if the last message was the level up cooldown.
                if(account.LastLevelUpTime.Subtract(now).TotalSeconds == Config.bot.LevelUpCooldown || account.LastLevelUpTime.Second == 0)
                {
                    LevelingSystem.UserSentMessage((SocketGuildUser)context.User, (SocketTextChannel)context.Channel, 10);

                    //We don't need to save the accounts file
                    account.LastLevelUpTime = now;
                }
            }
        }
    }
}
