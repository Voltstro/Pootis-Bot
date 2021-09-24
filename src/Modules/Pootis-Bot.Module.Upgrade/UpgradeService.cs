using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Core;
using Pootis_Bot.Logging;
using Pootis_Bot.Module.AutoVC;
using Pootis_Bot.Module.Basic;
using Pootis_Bot.Module.Profiles;
using Pootis_Bot.Module.RuleReaction;
using Pootis_Bot.Module.RuleReaction.Entities;
using Pootis_Bot.Module.Upgrade.OldEntities.Server;
using Pootis_Bot.Module.Upgrade.OldEntities.User;
using Pootis_Bot.Module.WelcomeMessage;
using Pootis_Bot.Module.WelcomeMessage.Entities;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Upgrade
{
    internal static class UpgradeService
    {
        public static void UpgradeConfigFiles(string oldConfigLocation)
        {
            Logger.Info("Upgrading config file...");
            //Upgrade the config now
            string configFile = Path.GetFullPath($"{oldConfigLocation}/Config.json");
            ConfigFile oldConfig = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(configFile));
            UpgradeConfigFile(oldConfig);
            Logger.Info("Done!");

            //Upgrade server list file
            string serverListFile = Path.GetFullPath($"{oldConfigLocation}/ServerList.json");
            if (File.Exists(serverListFile))
            {
                Logger.Info("Upgrading server list...");
                List<ServerList> serverList = JsonConvert.DeserializeObject<List<ServerList>>(File.ReadAllText(serverListFile));
                UpgradeServerList(serverList);
                Logger.Info("Done! {Count} server configurations upgraded.", serverList.Count);
            }
            
            //Upgrade the user accounts file
            string userAccountsFile = Path.GetFullPath($"{oldConfigLocation}/UserAccounts.json");
            if (File.Exists(userAccountsFile))
            {
                Logger.Info("Upgrading user accounts.");
                List<UserAccount> userAccounts =
                    JsonConvert.DeserializeObject<List<UserAccount>>(File.ReadAllText(userAccountsFile));
                UpgradeUserAccounts(userAccounts);
                Logger.Info("Done! {Count} user accounts upgraded.", userAccounts.Count);
            }
        }

        private static void UpgradeConfigFile(ConfigFile configFile)
        {
            BotConfig botConfig = BotConfig.Instance;
            botConfig.BotName = configFile.BotName;
            botConfig.BotPrefix = configFile.BotPrefix;
            botConfig.Save();

            if (ModuleManager.CheckIfModuleIsLoaded("ProfilesModule"))
                UpgradeProfileConfig(configFile);

            if (ModuleManager.CheckIfModuleIsLoaded("BasicModule"))
                UpgradeGameStatusConfig(configFile);
        }

        private static void UpgradeProfileConfig(ConfigFile configFile)
        {
            ProfilesConfig config = ProfilesConfig.Instance;
            config.XpGiveAmount = configFile.LevelUpAmount;
            config.XpGiveCooldown = TimeSpan.FromSeconds(configFile.LevelUpCooldown);
            config.Save();
        }

        private static void UpgradeGameStatusConfig(ConfigFile configFile)
        {
            GameStatusConfig config = GameStatusConfig.Instance;
            config.DefaultMessage = configFile.DefaultGameMessage;
            config.StreamingUrl = configFile.TwitchStreamingSite;
            config.Save();
        }

        #region User accounts

        private static void UpgradeUserAccounts(List<UserAccount> userAccounts)
        {
            foreach (UserAccount userAccount in userAccounts)
            {
                if (ModuleManager.CheckIfModuleIsLoaded("ProfilesModule"))
                    UpgradeProfile(userAccount);
            }
        }

        private static void UpgradeProfile(UserAccount userAccount)
        {
            ProfilesConfig config = ProfilesConfig.Instance;
            Profile profile = config.GetOrCreateProfile(userAccount.Id);
            profile.Xp = userAccount.Xp;
            profile.UserProfileMessage = userAccount.ProfileMsg;
            config.Save();
        }

        #endregion

        #region Serrver List

        private static void UpgradeServerList(List<ServerList> serverList)
        {
            foreach (ServerList server in serverList)
            {
                if (ModuleManager.CheckIfModuleIsLoaded("WelcomeMessageModule"))
                    UpgradeWelcomeMessage(server);

                if (ModuleManager.CheckIfModuleIsLoaded("RuleReactionModule"))
                    UpgradeRuleReaction(server);

                if (ModuleManager.CheckIfModuleIsLoaded("AutoVCModule"))
                    UpgradeAutoVc(server);
            }
        }

        private static void UpgradeWelcomeMessage(ServerList server)
        {
            WelcomeMessageConfig config = WelcomeMessageConfig.Instance;
            WelcomeMessageServer welcomeMessage = config.GetOrCreateWelcomeMessageServer(server.GuildId);
            welcomeMessage.GuildId = server.GuildId;
            welcomeMessage.WelcomeMessage = server.WelcomeMessage;
            welcomeMessage.GoodbyeMessage = server.WelcomeGoodbyeMessage;
            welcomeMessage.WelcomeMessageEnabled = server.WelcomeMessageEnabled;
            welcomeMessage.GoodbyeMessageEnabled = server.GoodbyeMessageEnabled;
            welcomeMessage.ChannelId = server.WelcomeChannelId;
            config.Save();
        }

        private static void UpgradeRuleReaction(ServerList server)
        {
            RuleReactionConfig config = RuleReactionConfig.Instance;
            RuleReactionServer reactionServer = config.GetOrCreateRuleReactionServer(server.GuildId);
            reactionServer.GuildId = server.GuildId;
            reactionServer.Emoji = server.RuleReactionEmoji;
            reactionServer.Enabled = server.RuleEnabled;
            reactionServer.ChannelId = server.RuleMessageChannelId;
            reactionServer.MessageId = server.RuleMessageId;
            reactionServer.RoleId = server.RuleRoleId;
            config.Save();
        }

        private static void UpgradeAutoVc(ServerList server)
        {
            AutoVCConfig config = AutoVCConfig.Instance;
            foreach (ServerAudioVoiceChannel serverAutoVc in server.AutoVoiceChannels)
            {
                if (!config.TryGetAutoVC(serverAutoVc.Id, out _))
                    config.AddAutoVc(serverAutoVc.Id, server.GuildId, serverAutoVc.Name);
            }
            config.Save();
        }
    }
    
    #endregion
}