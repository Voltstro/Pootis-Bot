using System;
using Discord.WebSocket;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
	public static class LevelingSystem
	{
		/// <summary>
		/// Gives a
		/// </summary>
		/// <param name="user"></param>
		/// <param name="channel"></param>
		/// <param name="amount"></param>
		public static async void UserSentMessage(SocketGuildUser user, SocketTextChannel channel, uint amount)
		{
			GlobalUserAccount userAccount = UserAccounts.GetAccount(user);
			uint oldLevel = userAccount.LevelNumber;

			//Nice one EternalClickbait...

			userAccount.Xp += amount;
			UserAccounts.SaveAccounts();

			if (oldLevel != userAccount.LevelNumber)
				await channel.SendMessageAsync($"{user.Mention} leveled up! Now on level **{userAccount.LevelNumber}**!");
		}
	}
}