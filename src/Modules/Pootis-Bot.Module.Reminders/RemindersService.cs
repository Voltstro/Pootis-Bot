using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Core;
using Pootis_Bot.Helper;
using Pootis_Bot.Logging;
using Pootis_Bot.Module.Reminders.Entities;

namespace Pootis_Bot.Module.Reminders
{
    public static class RemindersService
    {
        private static readonly RemindersConfig Config;
        private static readonly BotConfig BotConfig;

        static RemindersService()
        {
            Config ??= Config<RemindersConfig>.Instance;
            BotConfig ??= Config<BotConfig>.Instance;
        }
        
        public static void CreateAndStartReminder(SocketUser user, string message, IUserMessage messageData, IGuild guild, DateTime startTime, DateTime endTime, BaseSocketClient client)
        {
            Reminder reminder = Config.AddReminder(user.Id, message, messageData.GetMessageUrl(guild), startTime, endTime);
            Config.Save();
            StartReminder(reminder, client);
        }
        
        public static void StartReminder(Reminder reminder, BaseSocketClient client)
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
                    IDMChannel dm = await user.GetOrCreateDMChannelAsync();
                    if (dm != null)
                        await dm.SendEmbedAsync(embed);
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
    }
}