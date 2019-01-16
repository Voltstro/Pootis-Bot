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
        [Summary("Makes the user not warnable")]
        public async Task NotWarnable(IGuildUser user)
        {
            var server = ServerLists.GetServer(Context.Guild);
            var _user = Context.User as SocketGuildUser;

            if (server.permissions.PermNotWarnableRole != null && server.permissions.PermNotWarnableRole.Trim() != "")
            {
                var setrole = (_user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == server.permissions.PermNotWarnableRole);

                if(_user.Roles.Contains(setrole))
                    await Context.Channel.SendMessageAsync(MakeNotWarnable((SocketUser)user));
            }
            else //There isn't a set role, use deafult of the 'admin' role.
            {
                var deafultrole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == ServerLists.GetServer(_user.Guild).AdminRoleName);

                if (_user.Roles.Contains(deafultrole))
                    await Context.Channel.SendMessageAsync(MakeNotWarnable((SocketUser)user));
            }        
        }
    
        [Command("makewarnable")]
        [Summary("Makes the user warnable.")]
        public async Task MakeWarnable(IGuildUser user)
        {           
            var _user = Context.User as SocketGuildUser;
            var server = ServerLists.GetServer(Context.Guild);
            
            if (server.permissions.PermMakeWarnableRole != null && server.permissions.PermMakeWarnableRole.Trim() != "")
            {
                var setrole = (_user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == server.permissions.PermMakeWarnableRole);

                if(_user.Roles.Contains(setrole))
                    await Context.Channel.SendMessageAsync(MakeWarnable((SocketUser)user));
            }
            else //There isn't a set role, use deafult of the 'admin' role.
            {
                var deafultrole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == ServerLists.GetServer(_user.Guild).AdminRoleName);

                if (_user.Roles.Contains(deafultrole))
                    await Context.Channel.SendMessageAsync(MakeWarnable((SocketUser)user));
            }            
        }

        [Command("warn")]
        [Summary("Warns the user")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task WarnUser(IGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount((SocketGuildUser)user);
            var _user = Context.User as SocketGuildUser;
            var server = ServerLists.GetServer(Context.Guild);

            if (server.permissions.PermWarn != null && server.permissions.PermWarn.Trim() != "")
            {
                var setrole = (_user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == server.permissions.PermMakeWarnableRole);

                if (_user.Roles.Contains(setrole))
                {
                    await Context.Channel.SendMessageAsync(Warn((SocketUser)user));
                    await CheckWarnStatus((SocketGuildUser)user);                 
                }
            }
            else //There isn't a set role, use deafult of the 'staff' role.
            {
                var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == ServerLists.GetServer(_user.Guild).StaffRoleName);

                if (_user.Roles.Contains(role))
                {
                    await Context.Channel.SendMessageAsync(Warn((SocketUser)user));
                    await CheckWarnStatus((SocketGuildUser)user);
                }
            }    
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

            if (userAccount.IsAccountNotWarnable == true)
            {
                return $"The user {user} is already not warnable.";
            }
            else
            {
                userAccount.IsAccountNotWarnable = true;
                userAccount.Warnings = 0;
                UserAccounts.SaveAccounts();
                Console.WriteLine($"The user {user} was made not warnable.");
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
                Console.WriteLine($"The user {user} was made warnable.");
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
