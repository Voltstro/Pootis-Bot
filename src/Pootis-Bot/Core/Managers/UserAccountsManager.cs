using System;
using System.Collections.Generic;
using System.IO;
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
				if (CheckForOldFileName())
				{
					Global.Log("Renamed accounts.json to UserAccounts.json", ConsoleColor.Yellow);
					Accounts = DataStorage.LoadUserAccounts(AccountsFile).ToList();
					return;
				}

				Accounts = new List<UserAccount>();
				SaveAccounts();
			}
		}

		/// <summary>
		///     Saves all the accounts
		/// </summary>
		public static void SaveAccounts()
		{
			DataStorage.SaveUserAccounts(Accounts, AccountsFile);
		}

		/// <summary>
		///     Gets a user account, or creates one if needed
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

		//TODO: Remove this in the 1.0.0 release
		/// <summary>
		///     Checks for old file name (accounts.json)
		/// </summary>
		/// <returns>Returns true if upgraded</returns>
		private static bool CheckForOldFileName()
		{
			if (!DataStorage.SaveExists("Resources/accounts.json")) return false;

			File.Move("Resources/accounts.json", AccountsFile);
			return true;
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