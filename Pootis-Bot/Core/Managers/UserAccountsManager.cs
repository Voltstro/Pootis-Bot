using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core.Managers
{
	public static class UserAccountsManager
	{
		private const string AccountsFile = "Resources/accounts.json";
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
		/// Gets a user account, or creates one if needed
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static UserAccount GetAccount(SocketGuildUser user)
		{
			return GetOrCreateAccount(user);
		}

		public static UserAccount[] GetAllUserAccounts()
		{
			return Accounts.ToArray();
		}

		private static UserAccount GetOrCreateAccount(SocketGuildUser user)
		{
			IEnumerable<UserAccount> result = from a in Accounts
				where a.Id == user.Id
				select a;

			UserAccount account = result.FirstOrDefault();
			if (account == null) account = CreateUserAccount(user);
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

		public static async Task CheckUserWarnStatus(SocketGuildUser user)
		{
			if (user.IsBot)
				return;

			UserAccountServerData userAccount = GetAccount(user).GetOrCreateServer(user.Guild.Id);

			if (userAccount.Warnings >= 3) await user.KickAsync("Was kicked due to having 3 warnings.");

			if (userAccount.Warnings >= 4) await user.Guild.AddBanAsync(user, 5, "Was baned due to having 4 warnings.");
		}
	}
}