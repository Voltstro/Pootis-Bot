using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Pootis_Bot.Core.UserAccounts;

namespace Pootis_Bot.Core
{
    public static class DataStorage
    {
        // Save all userAccounts
        public static void SaveUserAccounts(IEnumerable<UserAccount> accounts, string filePath)
        {
            string json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // Get all userAccounts
        public static IEnumerable<UserAccount> LoadUserAccounts(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<UserAccount>>(json);
        }

        public static bool SaveExisits(string filePath)
        {
            return File.Exists(filePath);
        }

        public static void SaveServerList(IEnumerable<ServerList.ServerList> serverLists, string filePath)
        {
            string json = JsonConvert.SerializeObject(serverLists, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static IEnumerable<ServerList.ServerList> LoadServerList(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<ServerList.ServerList>>(json);
        }
    }
}
