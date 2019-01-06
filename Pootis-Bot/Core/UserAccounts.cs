using Discord.WebSocket;
using Pootis_Bot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pootis_Bot.Core
{
    public static class UserAccounts
    {
        private static List<GlobalUserAccount> accounts;

        private static string accountsFile = "Resources/accounts.json";

        static UserAccounts()
        {
            if (DataStorage.SaveExisits(accountsFile))
            {
                accounts = DataStorage.LoadUserAccounts(accountsFile).ToList();
            }
            else
            {
                accounts = new List<GlobalUserAccount>();
                SaveAccounts();
            }
        }

        public static void SaveAccounts()
        {
            DataStorage.SaveUserAccounts(accounts, accountsFile);
        }

        public static GlobalUserAccount GetAccount(SocketGuildUser user)
        {
            return GetOrCreateAccount(user.Id, user);
            
        }

        private static GlobalUserAccount GetOrCreateAccount(ulong id, SocketGuildUser user)
        {
            var result = from a in accounts
                         where a.ID == id
                         select a;

            var account = result.FirstOrDefault();
            if (account == null) account = CreateUserAccount(id, user);
            return account;
        }

        private static GlobalUserAccount CreateUserAccount(ulong id, SocketGuildUser user)
        {
            var newAccount = new GlobalUserAccount()
            {
                ID = id,
                Points = 10,
                XP = 0
            };

            newAccount.GetOrCreateServer(user.Guild.Id);

            accounts.Add(newAccount);
            SaveAccounts();
            return newAccount;
        }
    }
}
