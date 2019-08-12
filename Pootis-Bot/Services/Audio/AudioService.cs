using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Services.Audio
{
    public class AudioService
    {
        private readonly string ffmpegloc = "external/ffmpeg";
        private readonly string musicdir = "Music/";

        public readonly static List<GlobalServerMusicItem> CurrentChannels = new List<GlobalServerMusicItem>();

        public async Task JoinAudio(IGuild guild, IVoiceChannel target, IMessageChannel channel)
        {
            if (target == null)
            {
                await channel.SendMessageAsync(":musical_note: You need to be in a voice channel");
                return;
            }

            var audio = await target.ConnectAsync(); //Connect to the voice channel

            var item = new GlobalServerMusicItem //Added it to the CurrentChannels list
            {
                GuildID = guild.Id,
                IsPlaying = false,
                IsExit = false,
                AudioClient = audio,
                AudioChannel = (SocketVoiceChannel)target,
                StartChannel = (ISocketMessageChannel)channel,
                CancellationSource = new CancellationTokenSource()
            };

            CurrentChannels.Add(item);
        }

        public async Task LeaveAudio(IGuild guild, IMessageChannel channel)
        {
            if (guild == null) return; //Check if guild is null
            var ServerList = GetMusicList(guild.Id);
            if (ServerList == null)
            {
                await channel.SendMessageAsync(":musical_note: Your not in any voice channel!");
                return;
            }

            await ServerList.AudioClient.StopAsync(); //Stop the audio client
            ServerList.IsPlaying = false;

            CurrentChannels.Remove(GetMusicList(guild.Id)); //Remove it from the CurrentChannels list
        }

        public async Task StopAudioAsync(IGuild guild, IMessageChannel channel)
        {
            var ServerList = GetMusicList(guild.Id);

            if (ServerList == null)
            {
                await channel.SendMessageAsync(":musical_note: Your not in any voice channel!");
                return;
            }

            if (ServerList.IsPlaying == false)
            {
                await channel.SendMessageAsync(":musical_note: No audio is playing.");
            }

            ServerList.IsExit = true;
            await channel.SendMessageAsync(":musical_note: Stopping current playing song.");
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string search)
        {
            var ServerList = GetMusicList(guild.Id);

            if (ServerList == null)
            {
                await channel.SendMessageAsync(":musical_note: Your not in any voice channel!");
                return;
            }

            string fileName;
            string fileLoc = SearchAudio(search);   //Search for the song in our current music directory

            if (string.IsNullOrWhiteSpace(fileLoc)) //The search didn't come up with anything, lets attempt to get it from YouTube
            {
                AudioDownload audioDownload = new AudioDownload();
                string result = audioDownload.DownloadAudio(search, channel);
                if (result != null)
                {
                    fileLoc = result;
                }
                else
                    return;
            }

            fileLoc = fileLoc.Replace("\"", "'");

            string tempName = Path.GetFileName(fileLoc);
            fileName = tempName.Replace(".mp3", "");

            if (ServerList.IsPlaying)
            {
                ServerList.CancellationSource.Cancel();

                //Kill and dispose of ffmpeg
                ServerList.Ffmpeg.Kill();
                ServerList.Ffmpeg.Dispose();

                await ServerList.Discord.FlushAsync();

                while(ServerList.CancellationSource.IsCancellationRequested == true)
                {
                    await Task.Delay(100);
                    ServerList.CancellationSource.Dispose();
                    ServerList.CancellationSource = new CancellationTokenSource();
                }
            }

            var client = ServerList.AudioClient;
            var ffmpeg = ServerList.Ffmpeg = GetFfmpeg(fileLoc);

            Global.Log($"The song '{fileName}' on server {guild.Name}({guild.Id}) has started.", ConsoleColor.Blue);

            using (Stream output = ffmpeg.StandardOutput.BaseStream) //Start playing the song
            {
                using (ServerList.Discord = client.CreatePCMStream(AudioApplication.Music))
                {
                    ServerList.IsPlaying = true;
                    bool fail = false;
                    bool exit = false;
                    int bufferSize = 1024;
                    byte[] buffer = new byte[bufferSize];

                    CancellationToken cancellation = ServerList.CancellationSource.Token;

                    await channel.SendMessageAsync($":musical_note: Now playing **{fileName}**.");

                    ServerList.IsExit = false;

                    while (!fail && !exit)
                    {
                        try
                        {
                            if (cancellation.IsCancellationRequested)
                            {
                                exit = true;
                                break;
                            }

                            if (ServerList.IsExit == true)
                            {
                                exit = true;
                                break;
                            }

                            int read = await output.ReadAsync(buffer, 0, bufferSize, cancellation);
                            if (read == 0)
                            {
                                exit = true;
                                break;
                            }

                            await ServerList.Discord.WriteAsync(buffer, 0, read, cancellation);

                            if (ServerList.IsPlaying == false)
                            {
                                do
                                {
                                    //Do nothing, wait till isplaying is true
                                    await Task.Delay(100);
                                } while (ServerList.IsPlaying == false);
                            }
                        }
                        catch (OperationCanceledException)
                        {

                        }
                        catch (Exception ex)
                        {
                            await channel.SendMessageAsync($"Sorry an error occured **Error Details**\n{ex.Message}");
                            fail = true;
                        }
                    }

                    //End
                    Global.Log($"The song '{fileName}' on server {guild.Name}({guild.Id}) has stopped.", ConsoleColor.Blue);

                    await ServerList.Discord.FlushAsync();
                    ServerList.Discord.Dispose();
                    ServerList.IsPlaying = false;

                    if(!ServerList.CancellationSource.IsCancellationRequested)
                        await channel.SendMessageAsync($":musical_note: The song has finished.");

                    //Check to make sure that ffmpeg was disposed
                    ffmpeg.Dispose();
                }
            }
        }

        public async Task PauseAudio(IGuild guild, IMessageChannel channel)
        {
            if (guild == null) return; //Check guild if null

            var musicList = GetMusicList(guild.Id);
            if (musicList == null) return; //Check server list if it is null

            musicList.IsPlaying = !musicList.IsPlaying; //Toggle pause status

            if (musicList.IsPlaying) await channel.SendMessageAsync(":musical_note: Current song has been un-paused");
            else await channel.SendMessageAsync(":musical_note: Current song has been paused");

        }

        private string SearchAudio(string search)
        {
            if (!Directory.Exists(musicdir)) Directory.CreateDirectory(musicdir);

            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(musicdir);
            FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + search + "*.mp3");

            foreach (FileInfo foundFile in filesInDir)
            {
                string fullName = foundFile.FullName;
                return fullName;
            }

            return null;
        }

        private Process GetFfmpeg(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = ffmpegloc,
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        #region List Fuctions

        private GlobalServerMusicItem GetMusicList(ulong guildid)
        {
            var result = from a in CurrentChannels
                         where a.GuildID == guildid
                         select a;

            var list = result.FirstOrDefault();
            return list;
        }

        #endregion
    }
}
