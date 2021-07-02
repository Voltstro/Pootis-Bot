using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Pootis_Bot.Module.Reminders
{
    [Group("remind")]
    [Summary("Commands for reminders")]
    public class RemindersCommands : ModuleBase<SocketCommandContext>
    {
        [Command("me")]
        [Summary("Sets a reminder for you")]
        public async Task RemindMe(TimeSpan time, [Remainder] string message)
        {
            //Get end time
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = startTime.Add(time);

            RemindersService.CreateAndStartReminder(Context.User, message, Context.Message, Context.Guild, startTime, endTime, Context.Client);
            await Context.Channel.SendMessageAsync(
                $"Your reminder was set, I will remind you at {startTime:hh:mm:ss tt} UTC.");
        }
    }
}