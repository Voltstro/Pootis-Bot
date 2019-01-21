using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
    public static class DataStorage
    {
        // Save all userAccounts
        public static void SaveUserAccounts(IEnumerable<GlobalUserAccount> accounts, string filePath)
        {
            string json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // Get all userAccounts
        public static IEnumerable<GlobalUserAccount> LoadUserAccounts(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<GlobalUserAccount>>(json);
        }

        public static bool SaveExisits(string filePath)
        {
            return File.Exists(filePath);
        }

        public static void SaveServerList(IEnumerable<Entities.GlobalServerList> serverLists, string filePath)
        {
            string json = JsonConvert.SerializeObject(serverLists, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static IEnumerable<Entities.GlobalServerList> LoadServerList(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Entities.GlobalServerList>>(json);
        }
    }
}
