using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Services;

namespace Pootis_Bot.Modules.Basic
{
    public class Utils : ModuleBase<SocketCommandContext>
    {
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

        [Command("alluserroles")]
        [Summary("Gets all of a user's roles")]
        public async Task AllUserRoles(SocketGuildUser user)
        {
            var roles = user.Roles;
            StringBuilder allRoles = new StringBuilder();
            allRoles.Append($"{user.Username}'s roles: \n");

            var sortedRoles = roles.OrderByDescending(o => o.Position).ToList();

            foreach (var role in sortedRoles)
            {
                string roleName = role.Name;
                if (role.Position == 0)
                    roleName = "Default";

                allRoles.Append($"{roleName} | ");
            }

            await Context.Channel.SendMessageAsync(allRoles.ToString());
        }

        [Command("allroles")]
        [Summary("Gets all roles on the server")]
        public async Task GetAllRoles()
        {
            var roles = (Context.User as IGuildUser).Guild.Roles;
            StringBuilder allRoles = new StringBuilder();

            allRoles.Append($"All roles on this server: \n");

            var sortedRoles = roles.OrderByDescending(o => o.Position).ToList();

            foreach (var role in sortedRoles)
            {
                string roleName = role.Name;
                if (role.Position == 0)
                    roleName = "Default";

                allRoles.Append($"{roleName} | ");
            }

            await Context.Channel.SendMessageAsync(allRoles.ToString());
        }

        [Command("embedmessage")]
        [Alias("embed")]
        [Summary("Displays your message in an embed message")]
        public async Task CmdEmbedMessage(string title = "", [Remainder]string msg = "")
        {
            await Context.Channel.SendMessageAsync("", false, EmbedMessage(title, msg).Build());
        }

        [Command("ping")]
        [Summary("Ping Pong!")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync($"Pong! **{Context.Client.Latency}**ms");
        }

        #region Functions

        EmbedBuilder EmbedMessage(string title, string msg)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(title);
            embed.WithDescription(msg);

            return embed;
        }

        #endregion
    }
}
