using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;
using System;
using System.Threading.Tasks;

namespace Pootis_Bot.Services
{
    public class ReminderService
    {
        public static async Task RemindAsyncSeconds(SocketUser guild, int time, string msg)
        {
            int convert = (int)TimeSpan.FromSeconds(time).TotalMilliseconds;
            string timenow = Global.TimeNow();

            await Task.Delay(convert);

            var dm = await guild.GetOrCreateDMChannelAsync();

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Reminder");
            embed.WithDescription(msg);
            embed.WithFooter($"Reminder was set at {timenow}", guild.GetAvatarUrl());

            await dm.SendMessageAsync("", false, embed.Build());
        }
    }
}
