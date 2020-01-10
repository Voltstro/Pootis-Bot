using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services.AntiSpam;
using Pootis_Bot.TypeReaders;

namespace Pootis_Bot.Core
{
	public class CommandHandler
	{
		private const string UserNotFoundError = "User not found.";
		private const string UserNotFoundList = "This user doesn't exist: ";

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
		/// Install all the modules and sets up for command handling
		/// </summary>
		/// <returns></returns>
		public async Task SetupCommandHandlingAsync()
		{
			//Add our custom type readers
			_commands.AddTypeReader(typeof(string[]), new StringArrayTypeReader());
			_commands.AddTypeReader(typeof(SocketGuildUser[]), new GuildUserArrayTypeReader());

			//Add our modules and setup the module manager so other classes can access module info
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
			DiscordModuleManager.SetupModuleManager(_commands);

			_client.MessageReceived += HandleCommandAsync;
		}

		/// <summary>
		/// Checks all the help modules in the config
		/// </summary>
		public void CheckHelpModules()
		{
			foreach (string module in HelpModulesManager.GetHelpModules().SelectMany(helpModule =>
				helpModule.Modules.Where(module => DiscordModuleManager.GetModule(module) == null)))
				Logger.Log(
					$"There is no module called {module}! Reset the help modules or fix the help modules in the config file!",
					LogVerbosity.Error);
		}

		private async Task HandleCommandAsync(SocketMessage messageParam)
		{
			if (!(messageParam is SocketUserMessage msg)) return;

			if (msg.Author.IsBot) //Check to see if user is bot, if is bot return.
				return;

			SocketCommandContext context = new SocketCommandContext(_client, msg);

			//This message or command come in from a dm, not a guild, so just ignore it.
			if (context.Guild == null)
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

			if (server.BannedChannels.Any(item => msg.Channel.Id == item)) return;

			if (msg.HasStringPrefix(Global.BotPrefix, ref argPos)
			    || msg.HasMentionPrefix(Global.BotUser, ref argPos))
			{
				//Check user's permission to use command
				if (!CheckUserPermission(context, server, argPos))
				{
					await context.Channel.SendMessageAsync(
						"You do not have permission to use that command on this guild!");
					return;
				}

				//Result
				IResult result = await _commands.ExecuteAsync(context, argPos, _services);
				await HandleCommandResult(context, msg, result);
			}
			else
			{
				//Since it isn't a command we do level up stuff
				UserAccountServerData account = UserAccountsManager
					.GetAccount((SocketGuildUser) context.User).GetOrCreateServer(context.Guild.Id);
				DateTime now = DateTime.Now;

				HandleUserXpLevel(account, context, now);
				HandleUserPointsLevel(account, server, context, now);
			}
		}

		private bool CheckUserPermission(SocketCommandContext context, ServerList server, int argPos)
		{
			//Get the command first
			SearchResult cmdSearchResult = _commands.Search(context, argPos);
			if (!cmdSearchResult.IsSuccess) return true;

			ServerList.CommandPermission perm = server.GetCommandInfo(cmdSearchResult.Commands[0].Command.Name);
			if (perm == null) return true;
			
			//If they are an administrator they override permissions
			if(((SocketGuildUser) context.User).GuildPermissions.Administrator)
				return true;

			bool doesUserHavePerm = false;
			foreach (SocketRole role in ((SocketGuildUser) context.User).Roles)
			{
				if (doesUserHavePerm)
					continue;

				foreach (ulong unused in perm.Roles.Where(permRole =>
					role == RoleUtils.GetGuildRole(context.Guild, permRole)))
				{
					if (doesUserHavePerm)
						continue;

					doesUserHavePerm = true;
				}
			}

			return doesUserHavePerm || context.User.Id == context.Guild.OwnerId;
		}

		private static async Task HandleCommandResult(SocketCommandContext context, SocketUserMessage msg, IResult result)
		{
			//The command either had too little arguments or too many
			if (!result.IsSuccess && result.Error == CommandError.BadArgCount)
			{
				await context.Channel.SendMessageAsync(
					$"The command `{msg.Content.Replace(Global.BotPrefix, "")}` either has too many or too little arguments!");
			}

			//The user or bot had unmet preconditions
			else if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
			{
				await context.Channel.SendMessageAsync(result.ErrorReason);
			}

			//The user name imputed wasn't valid
			else if (!result.IsSuccess && result.Error == CommandError.ObjectNotFound &&
			         result.ErrorReason == UserNotFoundError)
			{
				await context.Channel.SendMessageAsync("You need to input a valid username!");
			}

			else if (!result.IsSuccess && result.Error == CommandError.ObjectNotFound &&
			         result.ErrorReason.StartsWith(UserNotFoundList))
			{
				await context.Channel.SendMessageAsync(
					$"The user `{result.ErrorReason.Replace(UserNotFoundList, "")}` wasn't found in this guild!");
			}

			//Some other error, just put the error into the console
			//and tell the user an internal error occured so they are not just left blank
			else if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
			{
				Logger.Log(result.ErrorReason, LogVerbosity.Error);
				await context.Channel.SendMessageAsync("Sorry, but an internal error occured.");

				//If the bot owner has ReportErrorsToOwner enabled we will give them a heads up about the error
				if (Config.bot.ReportErrorsToOwner)
					await Global.BotOwner.SendMessageAsync(
						$"ERROR: {result.ErrorReason}\nError occured while executing command `{msg.Content.Replace(Global.BotPrefix, "")}` on server `{context.Guild.Id}`.");
			}
		}

		#region User Level Up Stuff

		private static void HandleUserXpLevel(UserAccountServerData account, SocketCommandContext context, DateTime now)
		{
			//TODO: Rewrite this stuff

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

		private static void HandleUserPointsLevel(UserAccountServerData account, ServerList server, SocketCommandContext context, DateTime now)
		{
			//TODO: Rewrite this stuff

			//Server points
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (account.LastServerPointsTime.Subtract(now).TotalSeconds <= -server.PointsGiveCooldownTime ||
			    account.LastServerPointsTime.Second == 0)
			{
				LevelingSystem.GiveUserServerPoints((SocketGuildUser) context.User,
					(SocketTextChannel) context.Channel, server.PointGiveAmount);

				//No need to save since this variable has a JsonIgnore attribute
				account.LastServerPointsTime = now;
			}
		}

		#endregion
	}
}