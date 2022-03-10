using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Pootis_Bot.Module.Reminders
{
    [Group("remind", "Commands for reminders")]
    public class RemindersCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("me", "Sets a reminder for you")]
        public async Task RemindMe(TimeSpan time, string message)
        {
            await RespondAsync("Creating reminder...");
            
            //Get end time
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = startTime.Add(time);

            IUserMessage response = await GetOriginalResponseAsync();

            RemindersService.CreateAndStartReminder(Context.User, message, response, Context.Guild, startTime, endTime, Context.Client);
            
            await response.ModifyAsync(x => x.Content = $"Your reminder was set, I will remind you at {startTime:hh:mm:ss tt} UTC.");
        }
    }
}