using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Pootis_Bot.Config;

namespace Pootis_Bot.Module.Reminders;

[Group("remind", "Commands for reminders")]
public class RemindersInteractions : InteractionModuleBase<SocketInteractionContext>
{
    private readonly RemindersConfig config;

    public RemindersInteractions()
    {
        config = Config<RemindersConfig>.Instance;
    }
        
    [SlashCommand("me", "Sets a reminder for you")]
    public async Task RemindMe(TimeSpan time, string message)
    {
        TimeSpan minTime = config.ReminderMinTime;
        if (time > minTime)
        {
            await RespondAsync($"Remind time is too short! Needs to be greater then '{minTime:g}'.");
            return;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            await RespondAsync("Message is empty or just white space!");
            return;
        }
            
        await RespondAsync("Creating reminder...");
            
        //Get end time
        DateTime startTime = DateTime.UtcNow;
        DateTime endTime = startTime.Add(time);

        IUserMessage response = await GetOriginalResponseAsync();

        RemindersService.CreateAndStartReminder(Context.User, message, response, Context.Guild, startTime, endTime, Context.Client);
            
        await response.ModifyAsync(x => x.Content = $"Your reminder was set, I will remind you at {startTime:hh:mm:ss tt} UTC.");
    }
}