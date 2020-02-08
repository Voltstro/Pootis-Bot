using System;
using System.Collections.Generic;
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
		private readonly Dictionary<string, string> _errors = new Dictionary<string, string>
		{
			["User not found."] = "You need to input a valid username for your username argument!",
			["Failed to parse TimeSpan"] = "Your imputed time isn't in the right format, use a format like this: `1d 3h 40m 10s`"
		};

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

			_client.MessageReceived += HandleMessage;
		}

		private Task HandleMessage(SocketMessage messageParam)
		{
			//Check the message to make sure it isn't a bot or such and get the SocketUserMessage and context
			if(!CheckMessage(messageParam, out SocketUserMessage msg, out SocketCommandContext context))
				return Task.CompletedTask;

			ServerList server = ServerListsManager.GetServer(context.Guild);
			UserAccount user = UserAccountsManager.GetAccount((SocketGuildUser) context.User);
			
			//Checks the message with the anti spam services
			if(!CheckMessageSpam(msg, context, user))
				return Task.CompletedTask;

			//If the message is in a banned channel then ignore it
			if (server.BannedChannels.Any(item => msg.Channel.Id == item)) return Task.CompletedTask;

			//Handle the command
			if (HandleCommand(msg, context, server)) return Task.CompletedTask;

			//Since it isn't a command we do level up stuff
			UserAccountServerData account = UserAccountsManager
				.GetAccount((SocketGuildUser) context.User).GetOrCreateServer(context.Guild.Id);
			DateTime now = DateTime.Now;

			HandleUserXpLevel(user, context, now);
			HandleUserPointsLevel(account, server, context, now);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes the command
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="context"></param>
		/// <param name="server"></param>
		/// <returns>Returns false if it isn't a command with a prefix</returns>
		private bool HandleCommand(IUserMessage msg, SocketCommandContext context, ServerList server)
		{
			int argPos = 0;
			if (!msg.HasStringPrefix(Global.BotPrefix, ref argPos) &&
			    !msg.HasMentionPrefix(Global.BotUser, ref argPos)) return false;

			//Check user's permission to use command
			if (!CheckUserPermission(context, server, argPos))
			{ context.Channel.SendMessageAsync(
					"You do not have permission to use that command on this guild!").GetAwaiter().GetResult();
				return true;
			}

			//Execute the command and handle the result
			IResult result = _commands.ExecuteAsync(context, argPos, _services).GetAwaiter().GetResult(); 
			HandleCommandResult(context, msg, result).GetAwaiter().GetResult();

			return true;
		}

		/// <summary>
		/// Checks the message to make sure it isn't a bot and gets the <see cref="SocketUserMessage"/> and <see cref="SocketCommandContext"/>
		/// </summary>
		/// <param name="message">The message to check</param>
		/// <param name="msg">The <see cref="SocketUserMessage"/> to give back</param>
		/// <param name="context">The <see cref="SocketCommandContext"/></param>
		/// <returns>Returns false if it failed the check</returns>
		private bool CheckMessage(SocketMessage message, out SocketUserMessage msg, out SocketCommandContext context)
		{
			msg = null;
			context = null;
			if (!(message is SocketUserMessage userMessage)) return false;
			msg = userMessage;

			context = new SocketCommandContext(_client, msg);

			if (message.Author.IsBot)
				return false;

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (message.Author.IsWebhook)
				return false;

			return true;
		}

		/// <summary>
		/// Checks the message for spam, and if the user is muted
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="context"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		private bool CheckMessageSpam(SocketUserMessage msg, SocketCommandContext context, UserAccount user)
		{
			//TODO: Better muting system
			//Check if the user is muted, if so delete the message, oh and make sure it ISN'T the owner of the guild
			if (user.GetOrCreateServer(context.Guild.Id).IsMuted && user.Id != context.Guild.OwnerId)
			{
				msg.DeleteAsync().GetAwaiter().GetResult();
				return false;
			}

			//Someone has mention more than 2 users, check with the anti-spam
			if (msg.MentionedUsers.Count >= 2)
				if (_antiSpam.CheckMentionUsers(msg, context.Guild))
					return false;

			//There are role mentions
			if (msg.MentionedRoles.Count >= 1)
				if (_antiSpam.CheckRoleMentions(msg, (SocketGuildUser) msg.Author))
					return false;

			return true;
		}

		/// <summary>
		/// Check the user to see if they have permissions to use the command
		/// </summary>
		/// <param name="context"></param>
		/// <param name="server"></param>
		/// <param name="argPos"></param>
		/// <returns></returns>
		private bool CheckUserPermission(SocketCommandContext context, ServerList server, int argPos)
		{
			//Get the command first
			SearchResult cmdSearchResult = _commands.Search(context, argPos);
			if (!cmdSearchResult.IsSuccess) return true;

			ServerList.CommandPermission perm = server.GetCommandInfo(cmdSearchResult.Commands[0].Command.Name);
			if (perm == null) return true;
			
			//If they are an administrator they override permissions
			return ((SocketGuildUser) context.User).GuildPermissions.Administrator ||
			       ((SocketGuildUser) context.User).Roles.Any(role => perm.Roles.Any(permRole => role == RoleUtils.GetGuildRole(context.Guild, permRole)));
		}

		/// <summary>
		/// Handles the end command result, and whether or not we need to post an error
		/// </summary>
		/// <param name="context"></param>
		/// <param name="msg"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		private async Task HandleCommandResult(SocketCommandContext context, IMessage msg, IResult result)
		{
			//The user had unmet preconditions
			if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
			{
				await context.Channel.SendMessageAsync(result.ErrorReason);
				return;
			}

			//The command either had too little arguments or too many
			if (!result.IsSuccess && result.Error == CommandError.BadArgCount)
			{
				await context.Channel.SendMessageAsync(
					$"The command `{msg.Content.Replace(Global.BotPrefix, "")}` either has too many or too little arguments!");
				return;
			}

			if (!result.IsSuccess)
			{
				//Handle custom errors
				foreach (KeyValuePair<string, string> error in _errors.Where(error => result.ErrorReason.StartsWith(error.Key)))
				{
					await context.Channel.SendMessageAsync(error.Value);
					return;
				}
			}

			//Some other error, just put the error into the console
			//and tell the user an internal error occured so they are not just left blank
			if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
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

		private static void HandleUserXpLevel(UserAccount account, SocketCommandContext context, DateTime now)
		{
			if (!(now.Subtract(account.LastLevelUpTime).TotalSeconds >=
			      Config.bot.LevelUpCooldown)) return;

			//Give the user the XP
			LevelingSystem.GiveUserXp((SocketGuildUser)context.User, 
				(SocketTextChannel)context.Channel, Config.bot.LevelUpAmount);

			//Set the user's last level up time to now
			account.LastLevelUpTime = now;
		}

		private static void HandleUserPointsLevel(UserAccountServerData account, ServerList server, SocketCommandContext context, DateTime now)
		{
			//Server points
			if (!(now.Subtract(account.LastServerPointsTime).TotalSeconds >= 
			      server.PointsGiveCooldownTime)) return;

			LevelingSystem.GiveUserServerPoints((SocketGuildUser) context.User,
				(SocketTextChannel) context.Channel, server.PointGiveAmount);

			//No need to save since this variable has a JsonIgnore attribute
			account.LastServerPointsTime = now;
		}

		#endregion
	}
}