using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Services.AntiSpam;

namespace Pootis_Bot.Core
{
	public class CommandHandler
	{
		private readonly AntiSpamService _antiSpam;
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly IServiceProvider _services;

		public CommandHandler(DiscordSocketClient client)
		{
			_commands = new CommandService();
			_antiSpam = new AntiSpamService();
			_client = client;

			_services = new ServiceCollection().AddSingleton(this).BuildServiceProvider();
		}

		/// <summary>
		/// Install all the modules
		/// </summary>
		/// <returns></returns>
		public async Task InstallCommandsAsync()
		{
			_client.MessageReceived += HandleCommandAsync;

			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
		}

		/// <summary>
		/// Checks all the help modules in the config
		/// </summary>
		public void CheckHelpModules()
		{
			foreach (string module in HelpModulesManager.GetHelpModules().SelectMany(helpModule => helpModule.Modules.Where(module => GetModule(module) == null)))
			{
				Global.Log($"There is no module called {module}! Reset the help modules or fix the help modules in the config file!", ConsoleColor.Red);
			}
		}

		private async Task HandleCommandAsync(SocketMessage messageParam)
		{
			if (!(messageParam is SocketUserMessage msg)) return;

			if (msg.Author.IsBot) //Check to see if user is bot, if is bot return.
				return;

			SocketCommandContext context = new SocketCommandContext(_client, msg);

			//This message or command come in from a dm, not a guild, so just ignore it.
			if(context.Guild == null)
				return;

			ServerList server = ServerListsManager.GetServer(context.Guild);
			UserAccount user = UserAccountsManager.GetAccount((SocketGuildUser) context.User);
			int argPos = 0;

			//Check if the user is muted, if so delete the message, oh and make sure it ISN'T the owner of the guild
			if (user.GetOrCreateServer(context.Guild.Id).IsMuted && user.Id != context.Guild.OwnerId)
			{
				await msg.DeleteAsync();
				return;
			}

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

				ServerList.CommandInfo perm = server.GetCommandInfo(cmdSearchResult.Commands[0].Command.Name);
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

				//Result
				IResult result = await _commands.ExecuteAsync(context, argPos, _services);

				//The command either had too little arguments or too many
				if (!result.IsSuccess && result.Error == CommandError.BadArgCount)
					await context.Channel.SendMessageAsync(
						$"The command `{msg.Content.Replace(Global.BotPrefix, "")}` either has too many or too little arguments!");

				//The user or bot had unmet preconditions
				else if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
					await context.Channel.SendMessageAsync(result.ErrorReason);

				//Some other error, just put the error into the console
				//and tell the user an internal error occured so they are not just left blank
				else if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
				{
					Global.Log(result.ErrorReason, ConsoleColor.Red);
					await context.Channel.SendMessageAsync("Sorry, but an internal error occured.");

					//If the bot owner has ReportErrorsToOwner enabled we will give them a heads up about the error
					if (Config.bot.ReportErrorsToOwner)
					{
						await Global.BotOwner.SendMessageAsync(
							$"ERROR: {result.ErrorReason}\nError occured while executing command `{msg.Content.Replace(Global.BotPrefix, "")}` on server '{context.Guild.Id}'.");
					}
				}
			}
			else
			{
				//Since it isn't a command we do level up stuff

				UserAccountServerData account = UserAccountsManager
					.GetAccount((SocketGuildUser) context.User).GetOrCreateServer(context.Guild.Id);
				DateTime now = DateTime.Now;

				//Server points
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (account.LastServerPointsTime.Subtract(now).TotalSeconds <= -server.PointsGiveCooldownTime ||
				    account.LastServerPointsTime.Second == 0)
				{
					LevelingSystem.GiveUserServerPoints((SocketGuildUser)context.User, (SocketTextChannel) context.Channel, server.PointGiveAmount);

					//No need to save since this variable has a JsonIgnore attribute
					account.LastServerPointsTime = now;
				}

				//Only level it up if the last message was the level up cooldown.
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (account.LastLevelUpTime.Subtract(now).TotalSeconds <= -Config.bot.LevelUpCooldown ||
				    account.LastLevelUpTime.Second == 0)
				{
					LevelingSystem.UserSentMessage((SocketGuildUser) context.User, (SocketTextChannel) context.Channel,
						Config.bot.LevelUpAmount);

					//We don't need to save the accounts file since the LastLevelUpTime has a json ignore tag
					account.LastLevelUpTime = now;
				}
			}
		}

		/// <summary>
		/// Get a modules
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public ModuleInfo GetModule(string moduleName)
		{
			IEnumerable<ModuleInfo> result = from a in _commands.Modules
				where a.Name == moduleName
				select a;

			ModuleInfo module = result.FirstOrDefault();
			return module;
		}
	}
}