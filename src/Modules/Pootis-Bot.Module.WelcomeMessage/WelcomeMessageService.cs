using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Logging;
using Pootis_Bot.Module.WelcomeMessage.Entities;

namespace Pootis_Bot.Module.WelcomeMessage
{
    internal static class WelcomeMessageService
    {
        private static readonly WelcomeMessageConfig Config;
        
        static WelcomeMessageService()
        {
            Config ??= Config<WelcomeMessageConfig>.Instance;
        }

        public static void CheckAllServers(DiscordSocketClient client)
        {
            Logger.Debug("Checking welcome message servers...");
            for (int i = 0; i < Config.WelcomeMessageServers.Count; i++)
            {
                CheckServer(Config.WelcomeMessageServers[i].GuildId, client);
            }
        }
        
        public static bool CheckServer(ulong guildId, DiscordSocketClient client)
        {
            WelcomeMessageServer server = Config.GetOrCreateWelcomeMessageServer(guildId);
            return CheckServer(server, client);
        }

        public static bool CheckServer(WelcomeMessageServer server, DiscordSocketClient client)
        {
            SocketGuild guild = client.GetGuild(server.GuildId);
            if (guild == null)
            {
                Config.WelcomeMessageServers.Remove(server);
                Config.Save();
                
                Logger.Debug("Removed guild {GuildId} welcome message data as it no longer exists!", server.GuildId);
                return false;
            }

            if (!server.GoodbyeMessageEnabled && !server.WelcomeMessageEnabled)
                return true;

            SocketTextChannel channel = guild.GetTextChannel(server.ChannelId);
            if (channel == null)
            {
                server.ChannelId = 0;
                server.Disable();
                Config.Save();
                
                Logger.Debug("Disabled guild {GuildId} welcome message as the channel no longer exists!", server.GuildId);
                return false;
            }

            return true;
        }

        public static async Task UserJoined(SocketGuildUser user)
        {
            WelcomeMessageServer server = Config.GetOrCreateWelcomeMessageServer(user.Guild);
            if (!server.WelcomeMessageEnabled)
                return;

            string message = server.WelcomeMessage.Replace("%SERVER%", user.Guild.Name).Replace("%USER%", user.Mention);
            await user.Guild.GetTextChannel(server.ChannelId).SendMessageAsync(message);
        }
        
        public static async Task UserLeft(SocketGuild guild, SocketUser user)
        {
            WelcomeMessageServer server = Config.GetOrCreateWelcomeMessageServer(guild);
            if (!server.GoodbyeMessageEnabled)
                return;

            string message = server.GoodbyeMessage.Replace("%SERVER%", guild.Name).Replace("%USER%", user.Username);
            await guild.GetTextChannel(server.ChannelId).SendMessageAsync(message);
        }

        public static Task ChannelDeleted(SocketChannel channel)
        {
            WelcomeMessageServer server = Config.WelcomeMessageServers.FirstOrDefault(x => x.ChannelId == channel.Id);
            if(server == null)
                return Task.CompletedTask;
            
            if(!server.WelcomeMessageEnabled && !server.GoodbyeMessageEnabled)
                return Task.CompletedTask;

            if (server.ChannelId == channel.Id)
            {
                server.Disable();
                server.ChannelId = 0;
                Config.Save();
                Logger.Debug("Disabled guild {GuildId} welcome message as the channel no longer exists!", server.GuildId);
            }
            
            return Task.CompletedTask;
        }
    }
}