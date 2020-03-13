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
		/// Gets an user account by a ulong of their ID, and doesn't create server data if needed, also won't create an account if needed.
		/// So if the user doesn't exist all you will get is null.
		/// <para>Try and use <see cref="GetAccount"/> if possible!</para>
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static UserAccount GetAccountById(ulong id)
		{
			IEnumerable<UserAccount> result = from a in Accounts
				where a.Id == id
				select a;

			UserAccount account = result.FirstOrDefault();
			return account;
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

		/// <summary>
		/// Checks a user's warn status
		/// </summary>
		/// <param name="user"></param>
		public static void CheckUserWarnStatus(SocketGuildUser user)
		{
			if (user.IsBot)
				return;

			if (user.GuildPermissions.Administrator)
				return;

			UserAccountServerData userAccount = GetAccount(user).GetOrCreateServer(user.Guild.Id);
			ServerList server = ServerListsManager.GetServer(user.Guild);

			//Warnings needed for kick and ban are set to the same amount, and the user has enough warnings so just straight ban
			if (server.WarningsKickAmount == server.WarningsBanAmount && userAccount.Warnings >= server.WarningsKickAmount)
				user.BanUser((SocketUser)Global.BotUser, $"Banned for having {server.WarningsKickAmount} warnings.");

			//Enough warnings for a kick
			else if(userAccount.Warnings == server.WarningsKickAmount)
				user.KickUser((SocketUser) Global.BotUser, $"Kicked for having {server.WarningsKickAmount} warnings.");

			//Enough warnings for a ban
			else if (userAccount.Warnings >= server.WarningsBanAmount)
				user.BanUser((SocketUser) Global.BotUser, $"Banned for having {server.WarningsBanAmount} warnings.");
		}
	}
}