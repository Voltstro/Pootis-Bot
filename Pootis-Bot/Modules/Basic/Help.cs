using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Modules.Basic
{
	public class Help : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author   - Creepysin
		// Description      - The two help commands
		// Contributors     - Creepysin, 

		private readonly CommandService _cmdService;
		private readonly CommandHandler _commandHandler;

		public Help(CommandService commandService, CommandHandler cmdHandler)
		{
			_cmdService = commandService;
			_commandHandler = cmdHandler;
		}

		[Command("help")]
		[Alias("h")]
		[Summary("Gets help")]
		public async Task HelpCmd()
		{
			try
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(
					$"```# Pootis-Bot Normal Commands```\nFor more help on a specific command do `{Global.BotPrefix}help [command]`.\n");

				//Basic Commands
				foreach (HelpModule helpModule in HelpModulesManager.GetHelpModules())
				{
					builder.Append($"\n**{helpModule.Group}** - ");
					foreach (CommandInfo cmd in helpModule.Modules.SelectMany(module => _commandHandler.GetModule(module).Commands))
					{
						builder.Append($"`{cmd.Name}` ");
					}
				}

				await Context.Channel.SendMessageAsync(builder.ToString());
			}
			catch (NullReferenceException)
			{
				await Context.Channel.SendMessageAsync(
					$"Sorry, but it looks like the bot owner doesn't have the help options configured correctly.\nVisit {Global.websiteCommands} for command list.");

				Global.Log("The help options are configured incorrectly!", ConsoleColor.Red);
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

			SearchResult result = _cmdService.Search(Context, query);
			if (result.IsSuccess)
				foreach (CommandMatch command in result.Commands)
					embed.AddField(command.Command.Name,
						$"Summary: {command.Command.Summary}\nAlias: {FormatAliases(command.Command)}\nUsage: `{command.Command.Name} {FormatParms(command.Command)}`");

			if (embed.Fields.Count == 0)
				embed.WithDescription("Nothing was found for " + query);

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		private string FormatAliases(CommandInfo commandInfo)
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

		private string FormatParms(CommandInfo commandInfo)
		{
			IReadOnlyList<ParameterInfo> parms = commandInfo.Parameters;

			StringBuilder format = new StringBuilder();
			int count = parms.Count;
			if (count != 0) format.Append("[");
			int currentCount = 1;
			foreach (ParameterInfo parm in parms)
			{
				format.Append(parm);

				if (currentCount != count) format.Append(", ");
				currentCount += 1;
			}

			if (count != 0) format.Append("]");

			return format.ToString();
		}
	}
}