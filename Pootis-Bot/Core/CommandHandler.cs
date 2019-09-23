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
		private readonly AntiSpamService _antiSpam;
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;

		public CommandHandler(DiscordSocketClient client)
		{
			_commands = new CommandService();
			_antiSpam = new AntiSpamService();
			_client = client;
		}

		/// <summary>
		/// Adds a command to the list of commands
		/// </summary>
		/// <returns></returns>
		public async Task InstallCommandsAsync()
		{
			_client.MessageReceived += HandleCommandAsync;

			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
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
				if (_antiSpam.CheckMentionUsers(msg, context.Guild))
					return;

			//There are role mentions
			if (msg.MentionedRoles.Count >= 1)
				if (_antiSpam.CheckRoleMentions(msg, (SocketGuildUser) msg.Author))
					return;

			foreach (ulong item in server.BannedChannels) //Check to channel, make sure its not on the banned list
				if (msg.Channel.Id == item)
					return;
			if (msg.HasStringPrefix(Global.BotPrefix, ref argPos)
			    || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
			{
				//Permissions
				SearchResult cmdSearchResult = _commands.Search(context, argPos);
				if (!cmdSearchResult.IsSuccess) return;

				GlobalServerList.CommandInfo perm = server.GetCommandInfo(cmdSearchResult.Commands[0].Command.Name);
				if (perm != null)
				{
					bool doesUserHaveARole = false;

					foreach (string role in perm.Roles)
					{
						IRole guildRole = ((IGuildUser) context.User).Guild.Roles.FirstOrDefault(x => x.Name == role);
						if (((SocketGuildUser) context.User).Roles.Contains(guildRole)) doesUserHaveARole = true;
					}

					if (!doesUserHaveARole && (context.User.Id != context.Guild.Owner.Id))
						return;
				}

				IResult result = await _commands.ExecuteAsync(context, argPos, null);
				if (!result.IsSuccess && (result.Error == CommandError.BadArgCount))
					await context.Channel.SendMessageAsync(
						$"The command `{msg.Content.Replace(Global.BotPrefix, "")}` either has too many or too little arguments!");
				else if (!result.IsSuccess && (result.Error == CommandError.UnmetPrecondition))
					await context.Channel.SendMessageAsync(result.ErrorReason);
				else if (!result.IsSuccess && (result.Error != CommandError.UnknownCommand))
					Global.Log(result.ErrorReason, ConsoleColor.Red);
			}
			else
			{
				GlobalUserAccount.GlobalUserAccountServer account = UserAccounts
					.GetAccount((SocketGuildUser) context.User).GetOrCreateServer(context.Guild.Id);
				DateTime now = DateTime.Now;

				//Only level it up if the last message was the level up cooldown.
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if ((account.LastLevelUpTime.Subtract(now).TotalSeconds == Config.bot.LevelUpCooldown) ||
				    (account.LastLevelUpTime.Second == 0))
				{
					LevelingSystem.UserSentMessage((SocketGuildUser) context.User, (SocketTextChannel) context.Channel,
						10);

					//We don't need to save the accounts file
					account.LastLevelUpTime = now;
				}
			}
		}
	}
}