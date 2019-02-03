using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Services;

namespace Pootis_Bot.Modules
{
    public class BasicCommands : ModuleBase<SocketCommandContext>
    {
        [Command("hello")]
        [Summary("Displays the 'hello' message")]
        public async Task Hello()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Hello!");
            embed.WithDescription("Hello! My name is " + Config.bot.botName + "!\n\n**__Links__**" +
                "\n:computer: [Commands](https://creepysin.github.io/Pootis-Bot/commands/)" +
                "\n<:GitHub:529571722991763456> [Github Page](https://github.com/Creepysin/Pootis-Bot)" +
                "\n:bookmark: [Documation](https://creepysin.github.io/Pootis-Bot/)" +
                "\n<:Discord:529572497130127360> [Creepysin Development Server](https://discord.gg/m4YcsUa)" +
                "\n<:Discord:529572497130127360> [Creepysin Server](https://discord.gg/m7hg47t)");
            embed.WithColor(new Color(241, 196, 15));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("hasrole")]
        [Summary("Check if user has a role")]
        public async Task HasRole(string role, SocketGuildUser user)
        {
            var _role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == role);
            if (user.Roles.Contains(_role))
            {
                await Context.Channel.SendMessageAsync(user + " has the role '" + _role + "'");
            }
            else
            {
                await Context.Channel.SendMessageAsync(user + " Doesn't have the role '" + _role + "'");
            }
        }

        [Command("reminds")]
        [Summary("Reminds you, duh (In Seconds)")]
        [Alias("res")]
        public async Task Remind(int seconds, [Remainder] string remindmsg)
        {
            await Context.Channel.SendMessageAsync($"Ok, i will send you the message '{remindmsg}' in {seconds} seconds.");
            await ReminderService.RemindAsyncSeconds(Context.User, seconds, remindmsg);
        }
    }
}
