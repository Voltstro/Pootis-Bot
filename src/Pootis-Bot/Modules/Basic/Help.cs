using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Modules.Basic
{
	public class Help : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - The two help commands
		// Contributors     - Creepysin, 

		private readonly CommandService cmdService;

		public Help(CommandService commandService)
		{
			cmdService = commandService;
		}

		[Command("help")]
		[Alias("h")]
		[Summary("Gets help")]
		public async Task HelpCmd()
		{
			try
			{
				StringBuilder builder = new StringBuilder();
				List<string> existingCommands = new List<string>();
				builder.Append(
					$"```# Pootis-Bot Normal Commands```\nFor more help on a specific command do `{Global.BotPrefix}help [command]`.\n");

				//Basic Commands
				foreach (HelpModule helpModule in HelpModulesManager.GetHelpModules())
				{
					builder.Append($"\n**{helpModule.Group}** - ");
					foreach (CommandInfo command in helpModule.Modules.SelectMany(module =>
						DiscordModuleManager.GetModule(module).Commands))
					{
						if (existingCommands.Contains(command.Name)) continue;

						builder.Append($"`{command.Name}` ");
						existingCommands.Add(command.Name);
					}
				}

				await Context.Channel.SendMessageAsync(builder.ToString());
			}
			catch (NullReferenceException)
			{
				await Context.Channel.SendMessageAsync(
					$"Sorry, but it looks like the bot owner doesn't have the help options configured correctly.\nVisit {Global.websiteCommands} for command list.");

				Logger.Log("The help options are configured incorrectly!", LogVerbosity.Error);
			}
		}

		[Command("help")]
		[Alias("h", "command", "chelp", "ch")]
		[Summary("Gets help on a specific command")]
		public async Task HelpSpecific([Remainder] string query)
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"Help for {query}");
			embed.WithColor(new Color(241, 196, 15));

			SearchResult result = cmdService.Search(Context, query);
			if (result.IsSuccess)
				foreach (CommandMatch commandMatch in result.Commands)
					embed.AddField(commandMatch.Command.Name,
						$"**Summary**: {commandMatch.Command.Summary}\n" +
						$"**Alias**: {FormatAliases(commandMatch.Command)}\n" +
						$"**Usage**: `{commandMatch.Command.Name}{FormatParameters(commandMatch.Command)}`");

			if (embed.Fields.Count == 0)
				embed.WithDescription("Nothing was found for " + query);

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		private static string FormatAliases(CommandInfo commandInfo)
		{
			IReadOnlyList<string> aliases = commandInfo.Aliases;

			StringBuilder format = new StringBuilder();

			int count = aliases.Count;
			int currentCount = 1;
			foreach (string alias in aliases)
			{
				format.Append(alias);

				if (currentCount != count) format.Append(", ");
				currentCount += 1;
			}

			return format.ToString();
		}

		private static string FormatParameters(CommandInfo commandInfo)
		{
			IReadOnlyList<ParameterInfo> parameters = commandInfo.Parameters;

			if (parameters.Count == 0)
				return "";

			StringBuilder format = new StringBuilder();
			format.Append(" ");

			for (int i = 0; i < parameters.Count; i++)
			{
				if (parameters[i].IsOptional && !parameters[i].IsRemainder)
					format.Append($"[?{parameters[i].Name}]");
				else
					format.Append($"[{parameters[i].Name}]");

				if (i != parameters.Count - 1)
					format.Append(" ");
			}

			return format.ToString();
		}
	}
}