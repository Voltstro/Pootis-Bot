﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Helper;

namespace Pootis_Bot.Module.Basic
{
	[Group("help")]
	[Name("Help Commands")]
	[Summary("Provides help commands")]
	public class HelpCommands : ModuleBase<SocketCommandContext>
	{
		private readonly CommandService commandService;

		public HelpCommands(CommandService cmdService)
		{
			commandService = cmdService;
		}

		[Command]
		public async Task Help()
		{
			foreach (StringBuilder stringBuilder in BuildHelpMenu())
			{
				await Context.Channel.SendMessageAsync(stringBuilder.ToString());
			}
		}

		[Command]
		public async Task Help([Remainder] string query)
		{
			SearchResult searchResult = commandService.Search(Context, query);
			if (!searchResult.IsSuccess)
			{
				await Context.Channel.SendErrorMessageAsync("That command does not exist!");
				return;
			}

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"Help for `{query}`");
			foreach (CommandMatch match in searchResult.Commands)
			{
				embed.AddField(match.Command.Name,
					$"**Summary**: {match.Command.Summary}\n**Usage**: {BuildCommandUsage(match.Command)}");
			}

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		private StringBuilder[] BuildHelpMenu()
		{
			List<StringBuilder> groups = new List<StringBuilder>();
			foreach (ModuleInfo module in commandService.Modules)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("```diff\n");
				sb.Append($"+ {module.Name}\n");
				sb.Append($"  - Summary: {module.Summary}\n");

				foreach (CommandInfo command in module.Commands)
				{
					sb.Append($"\n- {command.Name}\n  - Summary: {command.Summary}\n  - Usage: {BuildCommandUsage(command)}");
				}

				sb.Append("\n```");
				groups.Add(sb);
			}

			return groups.ToArray();
		}

		private string BuildCommandUsage(CommandInfo command)
		{
			StringBuilder commandUsage = new StringBuilder();
			commandUsage.Append($"`{command.Name}");
			foreach (ParameterInfo parameter in command.Parameters)
			{
				commandUsage.Append($" <{parameter.Name}");
				if (parameter.DefaultValue != null)
				{
					commandUsage.Append($" = {parameter.DefaultValue}");
				}

				commandUsage.Append(">");
			}

			commandUsage.Append("`");
			return commandUsage.ToString();
		}
	}
}