using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Modules.Basic
{
    public class BasicCommands : ModuleBase<SocketCommandContext>
    {
        // Module Information
        // Original Author   - Creepysin
        // Description      - Basic, simple commands
        // Contributors     - Creepysin, 

        [Command("hello")]
        [Summary("Displays the 'hello' message")]
		[Cooldown(5)]
        public async Task Hello()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Hello!");
            embed.WithDescription("Hello! My name is " + Config.bot.BotName + "!\n\n**__Links__**" +
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

        [Command("server")]
        [Summary("Gets details about the server you are in")]
		[Cooldown(5)]
        public async Task ServerGuild()
        {
            var guildUser = (SocketGuildUser)Context.User;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Server Details");
            embed.WithDescription($"**__Server__**" +
                $"\n**Server Name:** {guildUser.Guild}" +
                $"\n**Server ID:** {guildUser.Guild.Id}" +
                $"\n**Server Member Count:** {guildUser.Guild.MemberCount}" +
                $"\n\n**__Server Owner__**" +
                $"\n**Owner Name: **{guildUser.Guild.Owner.Username}" +
                $"\n**Owner ID: ** {guildUser.Guild.OwnerId}");
            embed.WithThumbnailUrl(guildUser.Guild.IconUrl);
            embed.WithColor(new Color(241, 196, 15));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
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
            format.Append("```csharp\n 📋 Top 10 Server User Position\n ========================\n");

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
