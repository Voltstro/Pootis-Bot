using System;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.AntiSpam
{
	public class AntiSpamService
	{
		/// <summary>
		/// Checks how many users are mention in a single message, if it is higher then the threshold then remove it
		/// </summary>
		/// <param name="message">The message to check</param>
		/// <param name="guild">The guild of the message</param>
		/// <returns>Whether the user is allowed to do that action</returns>
		public bool CheckMentionUsers(SocketUserMessage message, SocketGuild guild)
		{
			SocketGuildUser user = (SocketGuildUser) message.Author;

			UserAccount.GlobalUserAccountServer serverAccount =
				UserAccounts.GetAccount(user).GetOrCreateServer(guild.Id);

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
				message.Channel.SendMessageAsync(
					$"Hey {message.Author.Mention}, saying a list of all the members of this Discord server is not allowed!");

				serverAccount.Warnings++;

				UserAccounts.CheckUserWarnStatus(user).GetAwaiter().GetResult();
				UserAccounts.SaveAccounts();

				return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if a given user is allowed to @mention a certain role, and warns them if not
		/// </summary>
		/// <param name="message">The message to check</param>
		/// <param name="user">The author of the message</param>
		/// <returns>Whether the user is allowed to do that action</returns>
		public bool CheckRoleMentions(SocketUserMessage message, SocketGuildUser user)
		{
			UserAccount.GlobalUserAccountServer serverAccount =
				UserAccounts.GetAccount(user).GetOrCreateServer(user.Guild.Id);

			if (serverAccount.IsAccountNotWarnable)
				return false;

			//If it is the owner of the Discord server, ignore
			if (user.Id == user.Guild.OwnerId)
				return false;

			ServerList server = ServerLists.GetServer(user.Guild);

			//Go over each role a user has
			foreach (SocketRole role in user.Roles)
			{
				foreach (RoleToRoleMention notToMentionRoles in server.RoleToRoleMentions)
					if (role.Id == notToMentionRoles.RoleNotToMentionId)
					{
						message.DeleteAsync();

						if (serverAccount.RoleToRoleMentionWarnings >=
						    server.AntiSpamSettings.RoleToRoleMentionWarnings)
						{
							message.Channel.SendMessageAsync(
								$"Hey {user.Mention}, you have been pinging the **{Global.GetGuildRole(user.Guild, notToMentionRoles.RoleId).Name}** role, which you are not allowed to ping!\nWe though we would tell you now and a warning has been added to your account, for info see your profile.");
							serverAccount.Warnings++;
							UserAccounts.SaveAccounts();
						}

						serverAccount.RoleToRoleMentionWarnings++;

						return true;
					}
			}

			return false;
		}
	}
}