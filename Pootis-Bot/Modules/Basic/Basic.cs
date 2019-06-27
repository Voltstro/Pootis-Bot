using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;
using Pootis_Bot.Services;

namespace Pootis_Bot.Modules
{
    public class BasicCommands : ModuleBase<SocketCommandContext>
    {
        // Module Infomation
        // Orginal Author   - Creepysin
        // Description      - Basic, simple commands
        // Contributors     - Creepysin, 

        [Command("hello")]
        [Summary("Displays the 'hello' message")]
        public async Task Hello()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Hello!");
            embed.WithDescription("Hello! My name is " + Config.bot.botName + "!\n\n**__Links__**" +
                $"\n:computer: [Commands]({Global.websiteCommands})" +
                $"\n<:GitHub:529571722991763456> [Github Page]({Global.githubPage})" +
                $"\n:bookmark: [Documation]({Global.websiteHome})" +
                $"\n<:Discord:529572497130127360> [Creepysin Development Server]({Global.discordServers[1]})" +
                $"\n<:Discord:529572497130127360> [Creepysin Server]({Global.discordServers[0]})" +
                $"\n\nRunning Pootis-Bot version: " + Global.version +
                $"\nThis project is licensed under the [MIT license]({Global.githubPage}/blob/master/LICENSE.md)");
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

        [Command("reminds")]
        [Summary("Reminds you, duh (In Seconds)")]
        [Alias("res")]
        public async Task Remind(int seconds, [Remainder] string remindmsg)
        {
            await Context.Channel.SendMessageAsync($"Ok, i will send you the message '{remindmsg}' in {seconds} seconds.");
            await ReminderService.RemindAsyncSeconds(Context.User, seconds, remindmsg);
        }

        [Command("allroles")]
        [Summary("Gets all roles on the server")]
        public async Task GetAllRoles()
        {
            var roles = (Context.User as IGuildUser).Guild.Roles;
            StringBuilder allRoles = new StringBuilder();

            allRoles.Append($"All roles on this server: \n");

            var sortedRoles = roles.OrderByDescending(o => o.Position).ToList();

            foreach(var role in sortedRoles)
            {
                string roleName = role.Name;
                if (role.Position == 0)
                    roleName = "Default";

                allRoles.Append($"{roleName} | ");
            }

            await Context.Channel.SendMessageAsync(allRoles.ToString());
        }

        [Command("top10")]
        [Summary("Get the top 10 users in the server")]
        public async Task Top10()
        {
            List<GlobalUserAccount> serverUsers = new List<GlobalUserAccount>();
            foreach(var user in Context.Guild.Users)
            {
                if(!user.IsBot && !user.IsWebhook)
                    serverUsers.Add(UserAccounts.GetAccount(user));
            }

            serverUsers.Sort(new SortUserAccount());
            serverUsers.Reverse();

            StringBuilder format = new StringBuilder();
            format.Append("```csharp\n 📋 Top 10 Server User Postitons\n ========================\n");

            int count = 1;
            foreach (var user in serverUsers)
            {
                if (count > 10)
                    continue;

                format.Append($"\n [{count}] -- # {Context.Client.GetUser(user.ID)}\n         └ Level: {user.LevelNumber}\n         └ XP: {user.XP}");
                count++;
            }

            var userAccount = UserAccounts.GetAccount((SocketGuildUser)Context.User);
            format.Append($"\n------------------------\n 😊 Your Level: {userAccount.LevelNumber}      Your XP: {userAccount.XP}```");
            await Context.Channel.SendMessageAsync(format.ToString());
        }

        private class SortUserAccount : IComparer<GlobalUserAccount>
        {
            public int Compare(GlobalUserAccount x, GlobalUserAccount y)
            {
                if (x.LevelNumber > y.LevelNumber)
                    return 1;
                else if (x.LevelNumber < y.LevelNumber)
                    return -1;
                else
                    return 0;
            }
        }
    }
}
