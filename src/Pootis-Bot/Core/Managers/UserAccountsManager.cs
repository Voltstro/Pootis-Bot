using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Core.Managers
{
	public static class UserAccountsManager
	{
		private const string AccountsFile = "Resources/UserAccounts.json";
		private static readonly List<UserAccount> Accounts;

		static UserAccountsManager()
		{
			if (DataStorage.SaveExists(AccountsFile))
			{
				Accounts = DataStorage.LoadUserAccounts(AccountsFile).ToList();
			}
			else
			{
				Accounts = new List<UserAccount>();
				SaveAccounts();
			}
		}

		/// <summary>
		/// Saves all the accounts
		/// </summary>
		public static void SaveAccounts()
		{
			DataStorage.SaveUserAccounts(Accounts, AccountsFile);
		}

		/// <summary>
		/// Gets a <see cref="UserAccount"/>, or creates one if needed
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static UserAccount GetAccount(SocketGuildUser user)
		{
			return GetOrCreateAccount(user);
		}

		/// <summary>
		/// Gets all <see cref="UserAccount"/>s
		/// </summary>
		/// <returns></returns>
		public static UserAccount[] GetAllUserAccounts()
		{
			return Accounts.ToArray();
		}

		private static UserAccount GetOrCreateAccount(SocketGuildUser user)
		{
			IEnumerable<UserAccount> result = from a in Accounts
				where a.Id == user.Id
				select a;

			UserAccount account = result.FirstOrDefault() ?? CreateUserAccount(user);
			return account;
		}

		private static UserAccount CreateUserAccount(SocketGuildUser user)
		{
			UserAccount newAccount = new UserAccount
			{
				Id = user.Id,
				Xp = 0,
				ProfileMsg = null,
				Servers = new List<UserAccountServerData>()
			};

			//Lets add the server that we are creating the account on
			newAccount.GetOrCreateServer(user.Guild.Id);

			Accounts.Add(newAccount);
			SaveAccounts();

			return newAccount;
		}

		public static void CheckUserWarnStatus(SocketGuildUser user)
		{
			if (user.IsBot)
				return;

			if (user.GuildPermissions.Administrator)
				return;

			UserAccountServerData userAccount = GetAccount(user).GetOrCreateServer(user.Guild.Id);

			if (userAccount.Warnings >= 3)
				UserUtils.KickUser(user, (SocketUser) Global.BotUser, "Kicked for having 3 warnings.");

			if (userAccount.Warnings >= 4)
				UserUtils.BanUser(user, (SocketUser) Global.BotUser, "Banned for having 3 warnings.");
		}
	}
}