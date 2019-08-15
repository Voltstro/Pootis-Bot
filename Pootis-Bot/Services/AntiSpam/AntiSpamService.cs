using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

			//If it is the owner of the Discord server or if the user is warnable, its probs safe to ignore
			if (user.Id == guild.OwnerId || !serverAccount.IsAccountNotWarnable)
				return false;

			int guildMemberCount = guild.Users.Count;
			int mentionCount = message.MentionedUsers.Count;

			if ((mentionCount / guildMemberCount) * 100 <= 45)
			{
				message.DeleteAsync();
				message.Channel.SendMessageAsync($"Hey {message.Author.Mention}, saying a list of all the members is not allowed!");

				serverAccount.Warnings++;

				UserAccounts.CheckUserWarnStatus(user).GetAwaiter().GetResult();
				UserAccounts.SaveAccounts();

				return true;
			}
			else
				return false;
		}

	}
}
