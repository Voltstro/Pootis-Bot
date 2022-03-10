using System;
using Cysharp.Text;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Console;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Module.Basic;

/// <summary>
///     Manager for the bot's profile game status
/// </summary>
public static class GameStatusManager
{
    private static DiscordSocketClient? discordClient;
    private static GameStatusConfig? config;
        
    internal static void OnConnected(DiscordSocketClient client)
    {
        discordClient = client;
        config = Config<GameStatusConfig>.Instance;
            
        if(!string.IsNullOrEmpty(config.DefaultMessage))
            SetStatus(config.DefaultMessage);
    }
        
    /// <summary>
    ///     Allows you to set the status to a custom message
    /// </summary>
    /// <param name="status">The status message</param>
    /// <param name="isStreaming">Show the bot as streaming or not?</param>
    /// <exception cref="NullReferenceException">Occurs when <see cref="isStreaming"/> is true and the streaming URL in the config is empty or null</exception>
    public static void SetStatus(string status, bool isStreaming = false)
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
    }

    [ConsoleCommand("setstatus", "Sets the status of a command")]
    internal static void SetStatusCommand(string[] args)
    {
        //Make sure our discord client isn't null
        if (discordClient == null)
        {
            Logger.Error("The client hasn't connected yet!");
            return;
        }

        //Check the args lenght
        if (args.Length < 2)
        {
            Logger.Error("This commands needs at least two arguments! [Streaming:True/False] [Status]");
            return;
        }

        //Get our first arg, streaming mode or not
        string streamingMode = args[0].ToLower();
        if (!bool.TryParse(streamingMode, out bool result))
        {
            Logger.Error("First argument needs to be either true or false!");
            return;
        }

        //Now to join the message without the first argument
        ReadOnlySpan<string> argsCut = args.AsSpan().Slice(1, args.Length - 1);
        string status = ZString.Join(' ', argsCut);
        try
        {
            SetStatus(status, result);
        }
        catch (NullReferenceException)
        {
            Logger.Error("The streaming URL in the game status config is empty or null!");
        }
    }
}