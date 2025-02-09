using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules;

/// <summary>
///     Module that provides the help command
/// </summary>
[Group("help", "Provides help commands")]
public class HelpModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InteractionService interactionService;
    private string[]? cachedHelpMenu;

    public HelpModule(InteractionService cmdService)
    {
        interactionService = cmdService;
    }

    [SlashCommand("get", "Gets help on commands")]
    public async Task Help(string? command = null)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            await RespondAsync("I will DM you the help info!");

            DmChat dmChat = new(Context.User);

            foreach (string message in GetHelpMenu())
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
        else
        {
            command = command.ToLowerInvariant();

            SlashCommandInfo[] searchResult = interactionService.SlashCommands.Where(x => x.Name == command).ToArray();
            if (searchResult.Length == 0)
            {
                await RespondAsync("No results where found!");
                return;
            }

            EmbedBuilder embed = new();
            embed.WithTitle($"Help for `{command}`");

            foreach (SlashCommandInfo commandInfo in searchResult)
                embed.AddField(commandInfo.Name,
                    $"**Summary**: {commandInfo.Description}\n**Usage**: {BuildCommandUsage(commandInfo)}");

            await RespondAsync(embed: embed.Build());
        }
    }

    private IEnumerable<string> GetHelpMenu()
    {
        if (cachedHelpMenu != null)
            return cachedHelpMenu;

        List<string> groups = new();
        ModuleInfo[] modules = interactionService.Modules.ToArray();
        foreach (ModuleInfo module in modules)
        {
            string message = $"```diff\n+ {module.Name}\n  - Summary: {module.Description}\n";
            message = module.SlashCommands.Aggregate(message,
                (current, command) =>
                    current +
                    $"\n- {BuildCommandFormat(command)}\n  - Summary: {command.Description}\n  - Usage: {BuildCommandUsage(command)}");
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
            {
                groups.Add(message);
            }
        }

        cachedHelpMenu = groups.ToArray();
        return cachedHelpMenu;
    }

    private string BuildCommandUsage(SlashCommandInfo command)
    {
        StringBuilder commandUsage = new StringBuilder();
        commandUsage.Append($"`{BuildCommandFormat(command)}");
        foreach (SlashCommandParameterInfo parameter in command.Parameters)
        {
            commandUsage.Append($" <{parameter.Name.ToLower()}");
            if (parameter.DefaultValue != null) commandUsage.Append($" = {parameter.DefaultValue}");

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