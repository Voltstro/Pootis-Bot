using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Pootis_Bot.Modules
{
    public class Basic : ModuleBase<SocketCommandContext>
    {
        [Command("hello")]
        public async Task Hello()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Hello!");
            embed.WithDescription("Hello! My name is " + Config.bot.botName + "!\n\n**__Links__**" +
                "\n:computer: [Commands](https://github.com/CreepysinProjects/Pootis-Bot/wiki/Pootis-Bot-Commands)" +
                "\n<:GitHub:529571722991763456> [Github Page](https://github.com/CreepysinProjects/Pootis-Bot)" +
                "\n<:Discord:529572497130127360> [Creepysin Development Server](https://discord.gg/m4YcsUa)" +
                "\n<:Discord:529572497130127360> [Creepysin Server](https://discord.gg/m7hg47t)");
            embed.WithColor(new Color(241, 196, 15));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("help")]
        public async Task Help()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(Context.User.Username);
            embed.WithDescription($"**Help for {Config.bot.botName}** \n\n :computer: [Commands](https://github.com/CreepysinProjects/Pootis-Bot/wiki/Pootis-Bot-Commands)\n\nFor support ask on my server <:Discord:529572497130127360> [Creepysin Server](https://discord.gg/m7hg47t)");
            embed.WithColor(new Color(0, 255, 0));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("hasrole")]
        public async Task HasRole(string _role, SocketGuildUser user)
        {
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == _role);
            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync(user + " has the role '" + _role + "'");
            }
            else
            {
                await Context.Channel.SendMessageAsync(user + " Doesn't have the role '" + _role + "'");
            }
        }
    }
}
