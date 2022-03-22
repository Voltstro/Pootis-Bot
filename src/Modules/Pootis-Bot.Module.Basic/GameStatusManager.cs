using System;
using Cysharp.Text;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Console;
using Pootis_Bot.Console.ConfigMenus;
using Pootis_Bot.Logging;
using Spectre.Console;

namespace Pootis_Bot.Module.Basic;

/// <summary>
///     Manager for the bot's profile game status
/// </summary>
public static class GameStatusManager
{
    private static DiscordSocketClient? discordClient;
    private static GameStatusConfig? config;

    private static string? currentStatus;
    private static bool currentStreaming;

    internal static void OnConnected(DiscordSocketClient client)
    {
        discordClient = client;
        config = Config<GameStatusConfig>.Instance;

        if (!string.IsNullOrEmpty(config.DefaultMessage))
            SetStatus(config.DefaultMessage);

        currentStatus = config.DefaultMessage;
        currentStreaming = false;
    }

    /// <summary>
    ///     Allows you to set the status to a custom message
    /// </summary>
    /// <param name="status">The status message</param>
    /// <param name="isStreaming">Show the bot as streaming or not?</param>
    /// <exception cref="NullReferenceException">
    ///     Occurs when <see cref="isStreaming" /> is true and the streaming URL in the
    ///     config is empty or null
    /// </exception>
    public static void SetStatus(string? status, bool isStreaming = false)
    {
        if (config == null)
            throw new ArgumentException("The client hasn't connected yet!");

        if (discordClient == null)
            throw new ArgumentException("The client hasn't connected yet!");

        string? streamingUrl = null;
        if (isStreaming)
        {
            if (string.IsNullOrEmpty(config.StreamingUrl))
                throw new NullReferenceException("The streaming URL in the config is empty or null!");

            streamingUrl = config.StreamingUrl;
        }

        ActivityType activityType = isStreaming ? ActivityType.Streaming : ActivityType.Playing;
        discordClient.SetGameAsync(status, streamingUrl, activityType);
        Logger.Info("Activity was set to '{Status}'", status);

        currentStatus = status;
        currentStreaming = isStreaming;
    }

    [ConsoleCommand("status_manage", "Sets the status of a command")]
    internal static void SetStatusCommand(string[] args)
    {
        //Make sure our discord client isn't null
        if (discordClient == null)
        {
            Logger.Error("The client hasn't connected yet!");
            return;
        }

        Rule rule = new("[blue]Manage Status[/]")
        {
            Alignment = Justify.Left
        };
        AnsiConsole.Write(rule);
        while (true)
        {
            AnsiConsole.Write("\n");
            string input = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Selection an option:")
                .AddChoices(new []{"Set Current Status", "Toggle Streaming Mode", "Status Config", "Exit"}));

            if (input == "Set Current Status")
            {
                string status = AnsiConsole.Ask<string>("What do you want to set the status to?");
                SetStatus(status, currentStreaming);
                AnsiConsole.WriteLine("Status successfully set!");
            }
            else if (input == "Toggle Streaming Mode")
            {
                SetStatus(currentStatus, !currentStreaming);
                AnsiConsole.WriteLine("Streaming status toggled.");
            }
            else if (input == "Status Config")
            {
                ConsoleConfigMenu<GameStatusConfig> statusConfig = new(config);
                statusConfig.Show();
                config.Save();
            }
            else if (input == "Exit")
            {
                AnsiConsole.WriteLine("Exited out of status management.");
                break;
            }
        }
    }
}