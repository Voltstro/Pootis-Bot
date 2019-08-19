﻿using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules.Basic
{
    public class ProfileMang : ModuleBase<SocketCommandContext>
    {
        // Module Information
        // Original Author   - Creepysin
        // Description      - Handles anything to do with profile managment
        // Contributors     - Creepysin, 

        [Command("makenotwarnable")]
        [Summary("Makes the user not warnable")]
        public async Task NotWarnable([Remainder] IGuildUser user = null)
        {
            await Context.Channel.SendMessageAsync(MakeNotWarnable((SocketUser)user));  
        }
    
        [Command("makewarnable")]
        [Summary("Makes the user warnable.")]
        public async Task MakeWarnable([Remainder] IGuildUser user = null)
        {           
            await Context.Channel.SendMessageAsync(MakeWarnable((SocketUser)user));            
        }

        [Command("warn")]
        [Summary("Warns the user")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task WarnUser(IGuildUser user)
        {
            await Context.Channel.SendMessageAsync(Warn((SocketUser)user));
            await UserAccounts.CheckUserWarnStatus((SocketGuildUser)user);
        }

        [Command("profile")]
        [Summary("Gets your")]
        public async Task Profile()
        {
            var roles = (Context.User as SocketGuildUser).Roles;
            var sortedRoles = roles.OrderByDescending(o => o.Position).ToList();
            var userMainRole = sortedRoles.First();

            var account = UserAccounts.GetAccount((SocketGuildUser)Context.User);
            var accountServer = account.GetOrCreateServer(Context.Guild.Id);
            var embed = new EmbedBuilder();

            string warningText = $"Yes";
            if (accountServer.IsAccountNotWarnable == true)
                warningText = $"No :sunglasses:";

            embed.WithCurrentTimestamp();
            embed.WithThumbnailUrl(Context.User.GetAvatarUrl());
            embed.WithTitle(Context.User.Username + "'s Profile");

            embed.AddField("Stats", $"**Level: ** {account.LevelNumber}\n**XP: ** {account.XP}\n", true);
            embed.AddField("Server", $"**Warnable: **{warningText}\n**Main Role: **{userMainRole.Name}\n", true);
            embed.AddField("Account", $"**ID: **{account.ID}\n**Creation Date: **{Context.User.CreatedAt}");

            embed.WithColor(userMainRole.Color);

            embed.WithFooter(account.Msg, Context.User.GetAvatarUrl());

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("profile")]
        [Summary("Gets a person's profile")]
        public async Task Profile(SocketGuildUser user)
        {
            var roles = (Context.User as SocketGuildUser).Roles;
            var sortedRoles = roles.OrderByDescending(o => o.Position).ToList();
            var userMainRole = sortedRoles.First();

            if(user.IsBot)
            {
                await Context.Channel.SendMessageAsync("You can not get a profile of a bot!");
                return;
            }

            var account = UserAccounts.GetAccount(user);
            var accountServer = account.GetOrCreateServer(Context.Guild.Id);
            var embed = new EmbedBuilder();

            string warningText = $"Yes";
            if (accountServer.IsAccountNotWarnable == true)
                warningText = $"No :sunglasses:";

            embed.WithCurrentTimestamp();
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            embed.WithTitle(user.Username + "'s Profile");

            embed.AddField("Stats", $"**Level: ** {account.LevelNumber}\n**XP: ** {account.XP}\n", true);
            embed.AddField("Server", $"**Warnable: **{warningText}\n**Main Role: **{userMainRole.Name}\n", true);
            embed.AddField("Account", $"**ID: **{account.ID}\n**Creation Date: **{user.CreatedAt}");

            embed.WithColor(userMainRole.Color);

            embed.WithFooter(account.Msg, user.GetAvatarUrl());

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("profilemsg")]
        public async Task ProfileMsg([Remainder]string message = "")
        {
            var account = UserAccounts.GetAccount((SocketGuildUser)Context.User);
            account.Msg = message;
            UserAccounts.SaveAccounts();

            await Context.Channel.SendMessageAsync($"Your public profile message was set to '{message}'");
        }
        
        #region Functions

        string MakeNotWarnable(SocketUser user)
        {
            if(user == null)
                return "That user doesn't exist!";

            if (user.IsBot)
                return "You can not change the warnable status of a bot!";

            SocketGuildUser userguild = (SocketGuildUser)user;
            var userAccount = UserAccounts.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);

            if (userAccount.IsAccountNotWarnable == true)
            {
                return $"**{user}** is already not warnable.";
            }
            else
            {
                userAccount.IsAccountNotWarnable = true;
                userAccount.Warnings = 0;
                UserAccounts.SaveAccounts();
                return $"**{user}** was made not warnable.";
            }
        }

        string MakeWarnable(SocketUser user)
        {
            if (user == null)
                return "That user doesn't exist!";

            if (user.IsBot)
                return "You can not change the warnable status of a bot!";

            SocketGuildUser userguild = (SocketGuildUser)user;

            var userAccount = UserAccounts.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);
            if (userAccount.IsAccountNotWarnable == false)
            {
                return $"**{user}** is already warnable.";
            }
            else
            {
                userAccount.IsAccountNotWarnable = false;
                UserAccounts.SaveAccounts();
                return $"**{user}** was made warnable.";
            }
        }

        string Warn(SocketUser user)
        {
            if (user.IsBot)
                return "You cannot give a warning to a bot!";

            SocketGuildUser userguild = (SocketGuildUser)user;
            var userAccount = UserAccounts.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);

            if (userAccount.IsAccountNotWarnable == true)
            {
                return $"A warning cannot be given to **{user}**. That person's account is set to not warnable.";
            }
            else
            {
                userAccount.Warnings++;
                UserAccounts.SaveAccounts();
                return $"A warning was given to **{user}**";
            }
        }

        #endregion
    }
}