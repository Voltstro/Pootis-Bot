using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Pootis_Bot.Commands.Permissions;
using Pootis_Bot.Config;
using Pootis_Bot.Core;
using Pootis_Bot.Discord;
using Pootis_Bot.Discord.TypeReaders;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Commands
{
	/// <summary>
	///     Handles commands for Discord
	/// </summary>
	internal sealed class CommandHandler
	{
		private readonly DiscordSocketClient client;
		private readonly CommandService commandService;
		private readonly BotConfig config;
		private readonly IServiceProvider serviceProvider;

		private readonly List<IPermissionProvider> permissionProviders;

		/// <summary>
		///     Creates a new <see cref="CommandHandler" /> instance
		/// </summary>
		/// <param name="client"></param>
		internal CommandHandler(DiscordSocketClient client)
		{
			config = Config<BotConfig>.Instance;
			this.client = client;
			client.MessageReceived += HandleMessage;

			commandService = new CommandService();
			commandService.AddTypeReader<Emoji>(new EmojiTypeReader());
			serviceProvider = new ServiceCollection()
				.AddSingleton(client)
				.AddSingleton(commandService)
				.BuildServiceProvider();
			permissionProviders = new List<IPermissionProvider>();
		}

		/// <summary>
		///     Install modules in an assembly
		/// </summary>
		/// <param name="assembly"></param>
		internal void InstallAssemblyModules(Assembly assembly)
		{
			commandService.AddModulesAsync(assembly, serviceProvider);
		}

		/// <summary>
		///		Adds a <see cref="IPermissionProvider"/>
		/// </summary>
		/// <param name="permissionProvider"></param>
		internal void AddPermissionProvider(IPermissionProvider permissionProvider)
		{
			permissionProviders.Add(permissionProvider);
		}

		private async Task HandleMessage(SocketMessage msg)
		{
			//Check the message first
			if (!CheckMessage(msg, out SocketUserMessage userMessage, out SocketCommandContext context)) return;

			//Does the message start with the prefix or mention of the bot
			int argPos = 0;
			if (!userMessage.HasStringPrefix(config.BotPrefix, ref argPos) &&
			    !userMessage.HasMentionPrefix(client.CurrentUser, ref argPos)) return;

			//First, find the command
			SearchResult searchResult = commandService.Search(context, argPos);
			if(!searchResult.IsSuccess)
				return;

			//Try to find the command
			CommandSearchResult commandSearchResult = await FindLikelyCommand(context, searchResult);
			if (!commandSearchResult.IsSuccess)
			{
				await context.Channel.SendMessageAsync(commandSearchResult.ErrorReason);
				return;
			}
			
			//Guild owners overrider everything
			if (context.User.Id != context.Guild.Owner.Id)
			{
				if (context.User is SocketGuildUser {GuildPermissions: {Administrator: false}})
				{
					//Check permissions with the command
					CommandPermissionResult permissionResult = await CheckCommandWithPermissionProviders(commandSearchResult.CommandMatch.Command, context);
					if (!permissionResult.IsSuccess)
					{
													
						await context.Channel.SendMessageAsync(permissionResult.ErrorReason);
						return;
					}
				}
			}

			//Execute the command
			IResult result = await commandSearchResult.CommandMatch.ExecuteAsync(context, commandSearchResult.ParseResult, serviceProvider);

			//Handle the result
			if (!result.IsSuccess && result.Error == CommandError.Exception)
			{
				await context.Channel.SendMessageAsync(
					"An internal error occurred while trying to handle your command!");
				Logger.Error($"An error occurred while handling a command! {result.ErrorReason}");
			}
		}

		private bool CheckMessage(SocketMessage message, out SocketUserMessage msg, out SocketCommandContext context)
		{
			msg = null;
			context = null;

			if (!(message is SocketUserMessage userMessage)) return false;
			msg = userMessage;

			context = new SocketCommandContext(client, msg);

			if (message.Author.IsBot || message.Author.IsWebhook)
				return false;

			return true;
		}

		private async Task<CommandPermissionResult> CheckCommandWithPermissionProviders(CommandInfo commandInfo, ICommandContext context)
		{
			foreach (IPermissionProvider permissionProvider in permissionProviders)
			{
				//Try/Catch this since its most likely talking with third-party module code
				try
				{
					PermissionResult result = await permissionProvider.OnExecuteCommand(commandInfo, context);
					if(!result.IsSuccess)
						return CommandPermissionResult.FromError(result.ErrorReason);
				}
				catch (Exception ex)
				{
					Logger.Error(ex, "An internal error occured with the permission provider {PermissionProviderName}!", permissionProvider.GetType().Name);
					return CommandPermissionResult.FromError("An internal error occured while checking the command's permissions!");
				}
			}

			return CommandPermissionResult.FromSuccess();
		}
		
		//99.9% of this code comes from Discord.NET: 
		//https://github.com/discord-net/Discord.Net/blob/22bb1b02dd9ec20c2485657f1b55193e18df393f/src/Discord.Net.Commands/CommandService.cs#L504
		private async Task<CommandSearchResult> FindLikelyCommand(ICommandContext context, SearchResult searchResult)
		{
			IReadOnlyList<CommandMatch> commands = searchResult.Commands;
			
			Dictionary<CommandMatch, PreconditionResult> preconditionResults = new Dictionary<CommandMatch, PreconditionResult>();
			foreach (CommandMatch match in commands)
			{
				preconditionResults[match] = await match.Command.CheckPreconditionsAsync(context, serviceProvider).ConfigureAwait(false);
			}

			KeyValuePair<CommandMatch, PreconditionResult>[] successfulPreconditions = preconditionResults
				.Where(x => x.Value.IsSuccess)
				.ToArray();

			if (successfulPreconditions.Length == 0)
			{
				//All preconditions failed, return the one from the highest priority command
				KeyValuePair<CommandMatch, PreconditionResult> bestCandidate = preconditionResults
					.OrderByDescending(x => x.Key.Command.Priority)
					.FirstOrDefault(x => !x.Value.IsSuccess);

				return CommandSearchResult.FromError(CommandError.UnmetPrecondition, "You lack the preconditions to run this command!");
			}
			
			//If we get this far, at least one precondition was successful.
			Dictionary<CommandMatch, ParseResult> parseResultsDict = new Dictionary<CommandMatch, ParseResult>();
			foreach (KeyValuePair<CommandMatch, PreconditionResult> pair in successfulPreconditions)
			{
				ParseResult parseResult = await pair.Key.ParseAsync(context, searchResult, pair.Value, serviceProvider).ConfigureAwait(false);

				parseResultsDict[pair.Key] = parseResult;
			}
			
			//Calculates the 'score' of a command given a parse result
			static float CalculateScore(CommandMatch match, ParseResult parseResult)
			{
				float argValuesScore = 0, paramValuesScore = 0;

				if (match.Command.Parameters.Count > 0)
				{
					float argValuesSum = parseResult.ArgValues?.Sum(x => x.Values.OrderByDescending(y => y.Score).FirstOrDefault().Score) ?? 0;
					float paramValuesSum = parseResult.ParamValues?.Sum(x => x.Values.OrderByDescending(y => y.Score).FirstOrDefault().Score) ?? 0;

					argValuesScore = argValuesSum / match.Command.Parameters.Count;
					paramValuesScore = paramValuesSum / match.Command.Parameters.Count;
				}

				float totalArgsScore = (argValuesScore + paramValuesScore) / 2;
				return match.Command.Priority + totalArgsScore * 0.99f;
			}
			
			//Order the parse results by their score so that we choose the most likely result to execute
			IOrderedEnumerable<KeyValuePair<CommandMatch, ParseResult>> parseResults = parseResultsDict
				.OrderByDescending(x => CalculateScore(x.Key, x.Value));

			KeyValuePair<CommandMatch, ParseResult>[] successfulParses = parseResults
				.Where(x => x.Value.IsSuccess)
				.ToArray();

			if (successfulParses.Length == 0)
			{
				//All parses failed, return the one from the highest priority command, using score as a tie breaker
				KeyValuePair<CommandMatch, ParseResult> bestMatch = parseResults
					.FirstOrDefault(x => !x.Value.IsSuccess);
				
				return CommandSearchResult.FromError(CommandError.ParseFailed, $"Fail to parse `{bestMatch.Value.ErrorParameter.Name}` argument! {bestMatch.Value.ErrorReason}");
			}

			//If we get this far, at least one parse was successful. Execute the most likely overload.
			KeyValuePair<CommandMatch, ParseResult> chosenOverload = successfulParses[0];
			return CommandSearchResult.FromSuccess(chosenOverload.Key, chosenOverload.Value);
		}
	}
}