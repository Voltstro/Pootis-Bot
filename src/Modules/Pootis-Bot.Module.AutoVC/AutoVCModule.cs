using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.AutoVC
{
    public class AutoVCModule : Modules.Module
    {
        public override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("AutoVCModule", "Voltstro", new Version(1, 0, 0));
        }

        public override Task ClientConnected(DiscordSocketClient client)
        {
            //Setup events
            client.ChannelDestroyed += channel =>
            {
                AutoVCService.DeleteChannel(channel);
                return Task.CompletedTask;
            };
            client.UserVoiceStateUpdated += (user, channelBefore, channelAfter) =>
            {
                //The user joined an auto VC channel
                if (AutoVCService.IsAutoVCChannel(channelAfter.VoiceChannel))
                {
                    _ = Task.Run(() => AutoVCService.CreateActiveSubAutoVC(channelAfter.VoiceChannel,
                        (SocketGuildUser) user,
                        channelAfter.VoiceChannel.Guild));
                }

                //The user left a channel
                if (channelBefore.VoiceChannel != null && channelAfter.VoiceChannel == null)
                {
                    _ = Task.Run(() => AutoVCService.RemoveActiveSubAutoVC(channelBefore.VoiceChannel));
                }

                return Task.CompletedTask;
            };
            
            //Check current channels
            client.Ready += () => PerformAutoVcChecks(client);

            return base.ClientConnected(client);
        }

        private static Task PerformAutoVcChecks(DiscordSocketClient client)
        {
            _ = Task.Run(() => AutoVCService.CheckAutoVCs(client));
            return Task.CompletedTask;
        }
    }
}