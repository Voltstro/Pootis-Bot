using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.ServerList;

namespace Pootis_Bot.Modules
{
    public class Basic : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(Context.User.Username);
            embed.WithDescription($"Help for {Config.bot.botName} \nMain - \nProfile - Profile\nFun - \nMisc - Pick, Creepysin");
            embed.WithColor(new Color(0, 255, 0));

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("checkrole")]
        public async Task CheckRole(string _role)
        {
            var user = Context.User as SocketGuildUser;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == _role);
            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync(Context.User + " has the role " + _role);
            }
            else
            {
                await Context.Channel.SendMessageAsync(Context.User + " Doesn't have the " + _role);
            }
        }
    }
}
