using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
    public static class UserAccounts
    {
        private static readonly List<GlobalUserAccount> Accounts;

        private const string AccountsFile = "Resources/accounts.json";

        static UserAccounts()
        {
            if (DataStorage.SaveExists(AccountsFile))
            {
                Accounts = DataStorage.LoadUserAccounts(AccountsFile).ToList();
            }
            else
            {
                Accounts = new List<GlobalUserAccount>();
                SaveAccounts();
            }
        }

        public static void SaveAccounts()
        {
            DataStorage.SaveUserAccounts(Accounts, AccountsFile);
        }

        public static GlobalUserAccount GetAccount(SocketGuildUser user)
        {
            return GetOrCreateAccount(user.Id, user); 
        }

        private static GlobalUserAccount GetOrCreateAccount(ulong id, SocketGuildUser user)
        {
            var result = from a in Accounts
                         where a.ID == id
                         select a;

            var account = result.FirstOrDefault();
            if (account == null) account = CreateUserAccount(id, user);
            return account;
        }

        private static GlobalUserAccount CreateUserAccount(ulong id, SocketGuildUser user)
        {
            var newAccount = new GlobalUserAccount
            {
                ID = id,
                XP = 0
            };

            newAccount.GetOrCreateServer(user.Guild.Id);

            Accounts.Add(newAccount);
            SaveAccounts();
            return newAccount;
        }

		public static async Task CheckUserWarnStatus(SocketGuildUser user)
		{
			if (user.IsBot)
				return;

			var userAccount = GetAccount(user).GetOrCreateServer(user.Guild.Id);

			if (userAccount.Warnings >= 3)
			{
				await user.KickAsync("Was kicked due to having 3 warnings.");
			}

			if (userAccount.Warnings >= 4)
			{
				await user.Guild.AddBanAsync(user, 5, "Was baned due to having 4 warnings.");
			}
		}
	}
}
