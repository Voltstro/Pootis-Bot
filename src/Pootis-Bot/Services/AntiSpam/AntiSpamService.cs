using System.Linq;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Services.AntiSpam
{
	public class AntiSpamService
	{
		/// <summary>
		///     Checks how many users are mention in a single message, if it is higher then the threshold then remove it
		/// </summary>
		/// <param name="message">The message to check</param>
		/// <param name="guild">The guild of the message</param>
		/// <returns>Whether the user is allowed to do that action</returns>
		public bool CheckMentionUsers(SocketUserMessage message, SocketGuild guild)
		{
			SocketGuildUser user = (SocketGuildUser) message.Author;

			UserAccountServerData serverAccount =
				UserAccountsManager.GetAccount(user).GetOrCreateServer(guild.Id);

			if (serverAccount.IsAccountNotWarnable)
				return false;

			//If it is the owner of the Discord server, ignore
			if (user.Id == guild.OwnerId)
				return false;

			//If user is a admin, ignore
			if (user.GuildPermissions.Administrator)
				return false;

			int guildMemberCount = guild.Users.Count;
			int mentionCount = message.MentionedUsers.Count;

			int percentage = mentionCount / guildMemberCount * 100;

			if (percentage > ServerListsManager.GetServer(guild).AntiSpamSettings.MentionUsersPercentage) return false;

			message.Channel.SendMessageAsync(
					$"Hey {message.Author.Mention}, listing all members of this Discord server is not allowed!")
				.GetAwaiter().GetResult();

			message.DeleteAsync().GetAwaiter().GetResult();

			serverAccount.Warnings++;

			UserAccountsManager.CheckUserWarnStatus(user);
			UserAccountsManager.SaveAccounts();

			return true;
		}

		/// <summary>
		///     Checks if a given user is allowed to @mention a certain role, and warns them if not
		/// </summary>
		/// <param name="message">The message to check</param>
		/// <param name="user">The author of the message</param>
		/// <returns>Whether the user is allowed to do that action</returns>
		public bool CheckRoleMentions(SocketUserMessage message, SocketGuildUser user)
		{
			UserAccountServerData serverAccount =
				UserAccountsManager.GetAccount(user).GetOrCreateServer(user.Guild.Id);

			if (serverAccount.IsAccountNotWarnable)
				return false;

			//If it is the owner of the Discord server, ignore
			if (user.Id == user.Guild.OwnerId)
				return false;

			ServerList server = ServerListsManager.GetServer(user.Guild);

			//Go over each role a user has
			foreach (SocketRole role in user.Roles)
			foreach (ServerRoleToRoleMention notToMentionRoles in server.RoleToRoleMentions.Where(notToMentionRoles =>
				role.Id == notToMentionRoles.RoleNotToMentionId))
			{
				message.DeleteAsync();

				if (serverAccount.RoleToRoleMentionWarnings >=
				    server.AntiSpamSettings.RoleToRoleMentionWarnings)
				{
					message.Channel.SendMessageAsync(
						$"Hey {user.Mention}, you have been pinging the **{RoleUtils.GetGuildRole(user.Guild, notToMentionRoles.RoleId).Name}** role, which you are not allowed to ping!\nWe though we would tell you now and a warning has been added to your account, for info see your profile.");
					serverAccount.Warnings++;
					UserAccountsManager.SaveAccounts();
				}

				serverAccount.RoleToRoleMentionWarnings++;

				return true;
			}

			return false;
		}
	}
}