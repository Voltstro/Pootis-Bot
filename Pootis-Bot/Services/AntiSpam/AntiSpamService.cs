using System;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services.AntiSpam
{
	public class AntiSpamService
	{
		public bool CheckMentionUsers(SocketUserMessage message, SocketGuild guild)
		{
			SocketGuildUser user = (SocketGuildUser)message.Author;

			var serverAccount = UserAccounts.GetAccount(user).GetOrCreateServer(guild.Id);

			if (serverAccount.IsAccountNotWarnable)
				return false;

			//If it is the owner of the Discord server, ignore
			if (user.Id == guild.OwnerId)
				return false;

			int guildMemberCount = guild.Users.Count;
			int mentionCount = message.MentionedUsers.Count;

			int percentage = (mentionCount / guildMemberCount) * 100;
			Console.WriteLine(percentage.ToString());

			if (percentage <= ServerLists.GetServer(guild).AntiSpamSettings.MentionUsersPercentage)
			{
				Console.WriteLine("Was more than 45 percent");

				message.DeleteAsync();
				message.Channel.SendMessageAsync($"Hey {message.Author.Mention}, saying a list of all the members of this Discord server is not allowed!");

				serverAccount.Warnings++;

				UserAccounts.CheckUserWarnStatus(user).GetAwaiter().GetResult();
				UserAccounts.SaveAccounts();

				return true;
			}
			else
				return false;
		}

		public bool CheckRoleMentions(SocketUserMessage message, SocketGuildUser user)
		{
			var serverAccount = UserAccounts.GetAccount(user).GetOrCreateServer(user.Guild.Id);

			if (serverAccount.IsAccountNotWarnable)
				return false;

			//If it is the owner of the Discord server, ignore
			if (user.Id == user.Guild.OwnerId)
				return false;

			var server = ServerLists.GetServer(user.Guild);

			//Go over each role a user has
			foreach(var role in user.Roles)
			{
				foreach(var notToMentionRoles in server.RoleToRoleMentions)
				{
					if(role.Name == notToMentionRoles.RoleNotToMention)
					{
						message.DeleteAsync();

						if(serverAccount.RoleToRoleMentionWarnings >= server.AntiSpamSettings.RoleToRoleMentionWarnings)
						{
							message.Channel.SendMessageAsync($"Hey {user.Mention}, you have been pinging the **{notToMentionRoles.Role}** role, which you are not allowed to ping!\nWe though we would tell you now and a warning has been added to your account, for info see your profile.");
							serverAccount.Warnings++;
							UserAccounts.SaveAccounts();
						}

						serverAccount.RoleToRoleMentionWarnings++;

						return true;
					}
				}
			}

			return false;
		}
	}
}
