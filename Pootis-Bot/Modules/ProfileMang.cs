using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules
{
    //Profile Managment commands

    public class ProfileMang : ModuleBase<SocketCommandContext>
    {      
        [Command("makenotwarnable")]
        public async Task NotWarnable(IGuildUser user)
        {
            var server = ServerLists.GetServer(Context.Guild);
            var _user = Context.User as SocketGuildUser;

            if (server.permNotWarnableRole != null && server.permNotWarnableRole.Trim() != "")
            {
                var setrole = (_user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == server.permNotWarnableRole);

                if(_user.Roles.Contains(setrole))
                    await Context.Channel.SendMessageAsync(MakeNotWarnable((SocketUser)user));
            }
            else //There isn't a set role, use deafult of the 'admin' role.
            {
                var deafultrole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == ServerLists.GetServer(_user.Guild).adminRoleName);

                if (_user.Roles.Contains(deafultrole))
                    await Context.Channel.SendMessageAsync(MakeNotWarnable((SocketUser)user));
            }        
        }
    
        [Command("makewarnable")]
        public async Task MakeWarnable(IGuildUser user)
        {           
            var _user = Context.User as SocketGuildUser;
            var server = ServerLists.GetServer(Context.Guild);
            
            if (server.permMakeWarnableRole != null && server.permMakeWarnableRole.Trim() != "")
            {
                var setrole = (_user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == server.permMakeWarnableRole);

                if(_user.Roles.Contains(setrole))
                    await Context.Channel.SendMessageAsync(MakeWarnable((SocketUser)user));
            }
            else //There isn't a set role, use deafult of the 'admin' role.
            {
                var deafultrole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == ServerLists.GetServer(_user.Guild).adminRoleName);

                if (_user.Roles.Contains(deafultrole))
                    await Context.Channel.SendMessageAsync(MakeWarnable((SocketUser)user));
            }            
        }

        [Command("warn")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task WarnUser(IGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount((SocketGuildUser)user);
            var _user = Context.User as SocketGuildUser;
            var server = ServerLists.GetServer(Context.Guild);

            if (server.permWarn != null && server.permWarn.Trim() != "")
            {
                var setrole = (_user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == server.permMakeWarnableRole);

                if (_user.Roles.Contains(setrole))
                {
                    await Context.Channel.SendMessageAsync(Warn((SocketUser)user));
                    await CheckWarnStatus((SocketGuildUser)user);                 
                }
            }
            else //There isn't a set role, use deafult of the 'staff' role.
            {
                var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == ServerLists.GetServer(_user.Guild).staffRoleName);

                if (_user.Roles.Contains(role))
                {
                    await Context.Channel.SendMessageAsync(Warn((SocketUser)user));
                    await CheckWarnStatus((SocketGuildUser)user);
                }
            }    
        }

        [Command("profile")]       
        public async Task Profile([Remainder]string arg = "")
        {
            SocketUser target = null;
            var metionUser = Context.Message.MentionedUsers.FirstOrDefault();
            target = metionUser ?? Context.User;

            var account = UserAccounts.GetAccount((SocketGuildUser)target);

            var guildtarget = (SocketGuildUser)target;
            var accountserver = account.GetOrCreateServer(guildtarget.Guild.Id);

            string WarningText = $"{target.Username} currently has {accountserver.warnings} warnings.";
            if (accountserver.isNotWarnable == true)
            {
                WarningText = $"{target.Username} account is not warnable.";
            }

            string Desciption = $"{target.Username} has {account.XP} XP. \n{target.Username} Has { account.Points} points. \n \n" + WarningText;
            var embed = new EmbedBuilder();     

            embed.WithThumbnailUrl(target.GetAvatarUrl());
            embed.WithTitle(target.Username + "'s Profile.");
            embed.WithDescription(Desciption);
            embed.WithColor(new Color(56, 56, 56));
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        #region Functions

        string MakeNotWarnable(SocketUser user)
        {
            SocketGuildUser userguild = (SocketGuildUser)user;
            var userAccount = UserAccounts.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);

            if (userAccount.isNotWarnable == true)
            {
                return $"The user {user} is already not warnable.";
            }
            else
            {
                userAccount.isNotWarnable = true;
                userAccount.warnings = 0;
                UserAccounts.SaveAccounts();
                Console.WriteLine($"The user {user} was made not warnable.");
                return $"The user {user} was made not warnable.";
            }
        }

        string MakeWarnable(SocketUser user)
        {
            SocketGuildUser userguild = (SocketGuildUser)user;

            var userAccount = UserAccounts.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);
            if (userAccount.isNotWarnable == false)
            {
                return $"The user {user} is already warnable.";
            }
            else
            {
                userAccount.isNotWarnable = false;
                UserAccounts.SaveAccounts();
                Console.WriteLine($"The user {user} was made warnable.");
                return $"The user {user} was made warnable.";
            }
        }

        string Warn(SocketUser user)
        {
            SocketGuildUser userguild = (SocketGuildUser)user;
            var userAccount = UserAccounts.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);

            if (userAccount.isNotWarnable == true)
            {
                return $"A warning cannot be given to {user}. That person's account is set to not warnable.";
            }
            else
            {
                userAccount.warnings++;
                UserAccounts.SaveAccounts();
                return $"A warning was given to {user}";
            }
        }

        async Task CheckWarnStatus(SocketGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount(user).GetOrCreateServer(user.Guild.Id);

            if (userAccount.warnings >= 3)
            {
                await user.KickAsync("Was kicked due to having 3 warnings.");
            }

            if (userAccount.warnings >= 4)
            {
                await user.Guild.AddBanAsync(user, 5, "Was baned due to having 4 warnings.");
            }
        }

        #endregion
    }
}
