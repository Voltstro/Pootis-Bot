using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Module.AutoVC
{
    /// <summary>
    ///     Main class for Auto VCs
    /// </summary>
    internal static class AutoVCService
    {
        private static readonly AutoVCConfig Config;
        
        static AutoVCService()
        {
            Config ??= Config<AutoVCConfig>.Instance;
        }
        
        /// <summary>
        ///     Checks if a channel is an auto VC and deletes it
        /// </summary>
        /// <param name="channel"></param>
        public static void DeleteChannel(SocketChannel channel)
        {
            try
            {
                if (!Config.TryGetAutoVC(channel.Id, out AutoVC autoVC)) return;
            
                Config.AutoVCs.Remove(autoVC);
                Config.Save();
                
                Logger.Debug("Auto VC {AutoVCId} was removed.", autoVC.ChannelId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Something went wrong while trying to remove a auto VC!");
            }
        }
        
        /// <summary>
        ///     Creates an active sub auto VC
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="user"></param>
        /// <param name="guild"></param>
        public static async Task CreateActiveSubAutoVC(SocketVoiceChannel channel, SocketGuildUser user, SocketGuild guild)
        {
            if (Config.TryGetAutoVC(channel.Id, out AutoVC autoVC))
            {
                //Setup and create the new channel
                RestVoiceChannel newChannel = await guild.CreateVoiceChannelAsync(
                    $"{autoVC.ChannelName} #{autoVC.ActiveSubAutoVc.Count + 1}",
                    properties =>
                    {
                        properties.CategoryId = channel.CategoryId;
                        properties.Bitrate = channel.Bitrate;
                        properties.Position = channel.Position + 1;
                    });
                if (newChannel.CategoryId != null)
                    await newChannel.SyncPermissionsAsync();
                
                //Add it to our list
                autoVC.ActiveSubAutoVc.Add(newChannel.Id);
                Config.Save();

                //Move the user into the new channel
                await user.ModifyAsync(x => { x.ChannelId = newChannel.Id; });
                Logger.Debug("Created the auto VC {AutoVCId} and moved user {UserId} to it.", channel.Id, user.Id);
            }
        }

        /// <summary>
        ///     Removes an active sub auto VC
        /// </summary>
        /// <param name="channel"></param>
        public static async Task RemoveActiveSubAutoVC(SocketVoiceChannel channel)
        {
            if(channel.Users.Count != 0)
                return;
            
            AutoVC autoVC = Config.AutoVCs.Find(x => x?.GuildId == channel.Guild.Id);
            if(autoVC == null)
                return;
            
            await channel.DeleteAsync();
            autoVC.ActiveSubAutoVc.Remove(channel.Id);
            Config.Save();
            Logger.Debug("Removed active auto VC {ActiveAutoVCId} as it had no users.", channel.Id);
        }

        /// <summary>
        ///     Checks all auto VCs to make sure they still exist
        /// </summary>
        /// <param name="client"></param>
        public static async Task CheckAutoVCs(DiscordSocketClient client)
        {
            try
            {
                Logger.Debug("Checking auto VCs...");

                List<AutoVC> autoVCs = Config.AutoVCs;
                for (int i = 0; i < autoVCs.Count; i++)
                {
                    //Get the Guild
                    SocketGuild guild = client.GetGuild(autoVCs[i].GuildId);
                    if (guild == null)
                    {
                        Logger.Debug("The guild {GuildId} doesn't exist anymore, removing auto VC data.",
                            autoVCs[i].GuildId);
                        autoVCs.RemoveAt(i);
                        continue;
                    }

                    //Check active auto sub VCs
                    for (int j = 0; j < autoVCs[i].ActiveSubAutoVc.Count; j++)
                    {
                        SocketVoiceChannel activeVc = guild.GetVoiceChannel(autoVCs[i].ActiveSubAutoVc[j]);
                        if (activeVc == null)
                        {
                            Logger.Debug(
                                "The active sub auto VC {ActiveSubVcId} doesn't exist anymore, removing active sub VC data.",
                                autoVCs[i].ActiveSubAutoVc[j]);
                            autoVCs[i].ActiveSubAutoVc.RemoveAt(j);
                            continue;
                        }

                        if (activeVc.Users.Count != 0) continue;

                        Logger.Debug("The active sub auto VC {ActiveSubVcId} doesn't have any users in it, deleting channel.", autoVCs[i].ActiveSubAutoVc[j]);
                        await activeVc.DeleteAsync();
                        autoVCs[i].ActiveSubAutoVc.RemoveAt(j);
                    }

                    //The auto VC doesn't exist anymore
                    if (guild.GetVoiceChannel(autoVCs[i].ChannelId) != null) continue;

                    Logger.Debug("The auto VC channel {AutoVCId} doesn't exist anymore, removing data.",
                        autoVCs[i].ChannelId);
                    autoVCs.RemoveAt(i);
                    continue;
                }

                Config.Save();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Something went wrong checking the auto VCs!");
            }
        }
        
        /// <summary>
        ///     Checks if an <see cref="SocketVoiceChannel"/> is an auto VC
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static bool IsAutoVCChannel(SocketVoiceChannel channel)
        {
            return channel != null && Config.TryGetAutoVC(channel.Id, out AutoVC _);
        }
    }
}