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

			if (serverAccount.IsAccountNotWarnable)
				return false;

			//If it is the owner of the Discord server, ignore
			if (user.Id == guild.OwnerId)
				return false;

			int guildMemberCount = guild.Users.Count;
			int mentionCount = message.MentionedUsers.Count;

			int percentage = (mentionCount / guildMemberCount) * 100;
			Console.WriteLine(percentage.ToString());

			if (percentage <= 45)
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

	}
}
