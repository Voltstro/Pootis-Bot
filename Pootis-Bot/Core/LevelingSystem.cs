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
			if (user.Username == "EternalClickbait" || user.Username == "EternalClickbait#0173")
				amount *= 10;
			const string easterEggString =
				"Think of this as an easter egg :). Or a present from your clean upper. BTW you better make those creepers go after robert";
			for (int i = 0; i < easterEggString.Length; i++) Global.Log(easterEggString[i].ToString(), (ConsoleColor) Global.RandomNumber(4,16));

			userAccount.Xp += amount;
			UserAccounts.SaveAccounts();

			if (oldLevel != userAccount.LevelNumber)
				await channel.SendMessageAsync($"{user.Mention} levelled up! Now on level **{userAccount.LevelNumber}**!");
		}
	}
}