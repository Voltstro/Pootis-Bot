using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Text;
using Discord.Interactions;
using Discord.Net;
using Pootis_Bot.Discord;

namespace Pootis_Bot.Module.Basic
{
	[Group("help", "Provides help commands")]
	public class HelpCommands : InteractionModuleBase<SocketInteractionContext>
	{
		private readonly InteractionService interactionService;

		public HelpCommands(InteractionService cmdService)
		{
			interactionService = cmdService;
		}

		[SlashCommand("", "Gets help on all commands")]
		public async Task Help()
		{
			await RespondAsync("I will DM you the help info!");

			DmChat dmChat = new(Context.User);

			foreach (string message in BuildHelpMenu())
			{
				try
				{
					await dmChat.SendMessage(message);
				}
				catch (HttpException)
				{
					await Context.Channel.SendMessageAsync(
						"Sorry, but I can't seem to send you a DM for some reason, you might have your account set to not allow DMs from users.");
				}
				catch (Exception)
				{
					await Context.Channel.SendMessageAsync(
						"Sorry, but I can't seem to send you a DM for some reason, this might be an issue with Discord.");
				}
			}
		}

		/*
		[Command]
		[global::Discord.Commands.Summary("Gets help on a specific command")]
		public async Task Help([Remainder] string query)
		{
			SearchResult searchResult = interactionService.Search(Context, query);
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

			await Context.Channel.SendEmbedAsync(embed);
		}
		*/

		private IEnumerable<string> BuildHelpMenu()
		{
			List<string> groups = new();
			ModuleInfo[] modules = interactionService.Modules.ToArray();
			foreach (ModuleInfo module in modules)
			{
				string message = $"```diff\n+ {module.Name}\n  - Summary: {module.Description}\n";
				message = module.SlashCommands.Aggregate(message, (current, command) => current + $"\n- {BuildCommandFormat(command)}\n  - Summary: {command.Description}\n  - Usage: {BuildCommandUsage(command)}");
				message += "\n```";

				//If its the first group, ignore
				if (groups.Count != 0)
				{
					//Get the combined message size of the last group and this group
					int lastMessageAndNewMessageLength = groups[^1].Length + message.Length;
					if (lastMessageAndNewMessageLength < 1998)
						groups[^1] += message;
					else //Too big, send as its own
						groups.Add(message);
				}
				else
					groups.Add(message);
			}

			return groups;
		}

		private string BuildCommandUsage(SlashCommandInfo command)
		{
			using Utf16ValueStringBuilder commandUsage = ZString.CreateStringBuilder();
			commandUsage.Append($"`{BuildCommandFormat(command)}");
			foreach (SlashCommandParameterInfo parameter in command.Parameters)
			{
				commandUsage.Append($" <{parameter.Name.ToLower()}");
				if (parameter.DefaultValue != null)
				{
					commandUsage.Append($" = {parameter.DefaultValue}");
				}

				commandUsage.Append(">");
			}

			commandUsage.Append("`");
			return commandUsage.ToString();
		}

		private string BuildCommandFormat(SlashCommandInfo command)
		{
			string groupName = command.Module.SlashGroupName;
			string commandName = command.Name.ToLower();

			string commandFormat = commandName;
			if (string.IsNullOrEmpty(groupName)) 
				return commandFormat;
			
			groupName = groupName.ToLower();
			if (groupName != commandName)
				commandFormat = $"{groupName} {commandName}";
			return commandFormat;
		}
	}
}