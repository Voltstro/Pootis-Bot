using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Console;
using Pootis_Bot.Core;
using Pootis_Bot.Discord;
using Pootis_Bot.Helper;
using Pootis_Bot.Logging;
using Pootis_Bot.Module.Reminders.Entities;

namespace Pootis_Bot.Module.Reminders;

/// <summary>
///     Service for reminders
/// </summary>
public static class RemindersService
{
    private static readonly RemindersConfig Config;
    private static readonly BotConfig BotConfig;

    static RemindersService()
    {
        Config ??= Config<RemindersConfig>.Instance;
        BotConfig ??= Config<BotConfig>.Instance;
    }

    /// <summary>
    ///     Starts all reminders that are in the config
    /// </summary>
    /// <param name="client"></param>
    internal static void StartAllReminders(BaseSocketClient client)
    {
        foreach (Reminder reminder in Config.Reminders)
        {
            StartReminder(reminder, client);
        }
    }
        
    /// <summary>
    ///     Creates and starts a reminder
    /// </summary>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <param name="messageData"></param>
    /// <param name="guild"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="client"></param>
    public static void CreateAndStartReminder(SocketUser user, string message, IUserMessage messageData, IGuild guild, DateTime startTime, DateTime endTime, BaseSocketClient client)
    {
        //Lots of null checks
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));

        if (messageData == null)
            throw new ArgumentNullException(nameof(messageData));

        if (guild == null)
            throw new ArgumentNullException(nameof(guild));

        if (client == null)
            throw new ArgumentNullException(nameof(client));
            
        Reminder reminder = Config.AddReminder(user.Id, message, messageData.GetMessageUrl(guild), startTime, endTime);
        Config.Save();
        StartReminder(reminder, client);
    }

    private static void StartReminder(Reminder reminder, BaseSocketClient client)
    {
        _ = Task.Run(() => StartReminderAsync(reminder, client));
    }

    private static async Task StartReminderAsync(Reminder reminder, BaseSocketClient client)
    {
        TimeSpan timeDifference = reminder.EndTime.Subtract(DateTime.UtcNow);
        int milliseconds = (int) timeDifference.TotalMilliseconds;
            
        if (milliseconds > 0)
            await Task.Delay(milliseconds);

        try
        {
            SocketUser user = client.GetUser(reminder.UserId);
            if (user != null)
            {
                Config.Reminders.Remove(reminder);
                Config.Save();
                    
                //Build our message
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle($"{BotConfig.BotName} Reminder");
                embed.WithAuthor($"Reminder set at: {reminder.StartTime:hh:mm:ss tt} UTC", client.CurrentUser.GetAvatarUrl(), reminder.MessageUrl);
                embed.WithDescription($"\"{reminder.Message}\"");
                DmChat dmChat = new(user);
                try
                {
                    await dmChat.SendMessage(embed);
                }
                catch (Exception)
                {
                    //All good, logged within dmChat.SendMessage
                }
            }
            else
            {
                Logger.Warn("Failed to get a user's account for a reminder. It is more then likely it was deleted.");
            }
                
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Something went wrong while handling a reminder!");
        }
    }

    [ConsoleCommand("set_remind_min_time", "Sets the min time for a reminder")]
    internal static void SetMinReminderTime(string[] args)
    {
        if (args.Length <= 0)
        {
            Logger.Error("One input is required!");
            return;
        }

        if (!TimeSpan.TryParse(args[0], out TimeSpan result))
        {
            Logger.Error("Input is not a valid time!");
            return;
        }

        Config.ReminderMinTime = result;
        Config.Save();
            
        Logger.Info("Reminder min time was set to {Time}", result.ToString("g"));
    }
}