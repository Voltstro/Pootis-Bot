using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules
{
    public class ProfileMang : ModuleBase<SocketCommandContext>
    {
        // Module Infomation
        // Orginal Author   - Creepysin
        // Description      - Handles anything to do with profile managment
        // Contributors     - Creepysin, 

        [Command("makenotwarnable")]
        [Summary("Makes the user not warnable")]
        public async Task NotWarnable(IGuildUser user)
        {
            await Context.Channel.SendMessageAsync(MakeNotWarnable((SocketUser)user));  
        }
    
        [Command("makewarnable")]
        [Summary("Makes the user warnable.")]
        public async Task MakeWarnable(IGuildUser user)
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
            await CheckWarnStatus((SocketGuildUser)user);
        }

        [Command("profile")]
        [Summary("Gets your or other's profile")]
        public async Task Profile([Remainder]string user = "")
        {
            SocketUser target = null;
            var metionUser = Context.Message.MentionedUsers.FirstOrDefault();
            target = metionUser ?? Context.User;

            var account = UserAccounts.GetAccount((SocketGuildUser)target);

            var guildtarget = (SocketGuildUser)target;
            var accountserver = account.GetOrCreateServer(guildtarget.Guild.Id);

            string WarningText = $"{target.Username} currently has {accountserver.Warnings} warnings.";
            if (accountserver.IsAccountNotWarnable == true)
            {
                WarningText = $"{target.Username} account is not warnable.";
            }

            string Desciption = $"{target.Username} has {account.XP} XP. \n{target.Username} Has {account.Points} points. \n '{account.Msg}'\n\n" + WarningText;
            var embed = new EmbedBuilder();
            embed.WithThumbnailUrl(target.GetAvatarUrl());
            embed.WithTitle(target.Username + "'s Profile.");
            embed.WithDescription(Desciption);
            embed.WithColor(new Color(56, 56, 56));
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("profilemsg")]
        public async Task ProfileMsg([Remainder]string message = "")
        {
            var account = UserAccounts.GetAccount((SocketGuildUser)Context.User);
            account.Msg = message;
            UserAccounts.SaveAccounts();

            await Context.Channel.SendMessageAsync($"Your profile message was set to '{message}'");
        }

        #region Functions

        string MakeNotWarnable(SocketUser user)
        {
            SocketGuildUser userguild = (SocketGuildUser)user;
            var userAccount = UserAccounts.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);

            if (userAccount.IsAccountNotWarnable == true)
            {
                return $"The user {user} is already not warnable.";
            }
            else
            {
                userAccount.IsAccountNotWarnable = true;
                userAccount.Warnings = 0;
                UserAccounts.SaveAccounts();
                return $"The user {user} was made not warnable.";
            }
        }

        string MakeWarnable(SocketUser user)
        {
            SocketGuildUser userguild = (SocketGuildUser)user;

            var userAccount = UserAccounts.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);
            if (userAccount.IsAccountNotWarnable == false)
            {
                return $"The user {user} is already warnable.";
            }
            else
            {
                userAccount.IsAccountNotWarnable = false;
                UserAccounts.SaveAccounts();
                return $"The user {user} was made warnable.";
            }
        }

        string Warn(SocketUser user)
        {
            SocketGuildUser userguild = (SocketGuildUser)user;
            var userAccount = UserAccounts.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);

            if (userAccount.IsAccountNotWarnable == true)
            {
                return $"A warning cannot be given to {user}. That person's account is set to not warnable.";
            }
            else
            {
                userAccount.Warnings++;
                UserAccounts.SaveAccounts();
                return $"A warning was given to {user}";
            }
        }

        async Task CheckWarnStatus(SocketGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount(user).GetOrCreateServer(user.Guild.Id);

            if (userAccount.Warnings >= 3)
            {
                await user.KickAsync("Was kicked due to having 3 warnings.");
            }

            if (userAccount.Warnings >= 4)
            {
                await user.Guild.AddBanAsync(user, 5, "Was baned due to having 4 warnings.");
            }
        }

        #endregion
    }
}
