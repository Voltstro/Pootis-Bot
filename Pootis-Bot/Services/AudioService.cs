using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Pootis_Bot.Entities;

public class AudioService
{
    private readonly string ffmpegloc = "external/ffmpeg.exe";

    private static List<GlobalServerMusicItem> CurrentChannels = new List<GlobalServerMusicItem>();

    public async Task JoinAudio(IGuild guild, IVoiceChannel target)
    {
        var audio = await target.ConnectAsync(); //Connect to the voice channel

        var item = new GlobalServerMusicItem //Added it to the CurrentChannels list
        {
            GuildID = guild.Id,
            IsPlaying = false,
            AudioClient = audio
        };

        CurrentChannels.Add(item);
    }

    public async Task LeaveAudio(IGuild guild)
    {
        if (guild == null) return; //Check if guild is null

        await GetMusicList(guild.Id).AudioClient.StopAsync(); //Stop the audio client
        GetMusicList(guild.Id).IsPlaying = false;

        CurrentChannels.Remove(GetMusicList(guild.Id)); //Remove it from the CurrentChannels list
    }

    public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string search)
    {
        var ServerList = GetMusicList(guild.Id);

        var searchResults = SearchAudio(search); //Search to see if we might allready have the song
        string results;

        if(searchResults == null) //If we don't have the song then attempt to download it from youtube
        {
            AudioDownload download = new AudioDownload();
            results = download.DownloadAudio(search, channel);
            if (results == null)
            {
                await channel.SendMessageAsync($"Failed to download the song '{search}'\n");
                return;
            }  
        }
        else
            results = searchResults;

        var client = ServerList.AudioClient;

        Process ffmpeg = GetFfmpeg(results);

        using (Stream output = ffmpeg.StandardOutput.BaseStream) //Start playing the song
        {
            using (AudioOutStream discord = client.CreatePCMStream(AudioApplication.Music))
            {
                ServerList.IsPlaying = true;
                bool fail = false;
                bool exit = false;
                int bufferSize = 1024;
                byte[] buffer = new byte[bufferSize];

                CancellationToken cancellation = new CancellationToken();

                await channel.SendMessageAsync($"Now playing '{search}'");
                
                while(!fail && !exit)
                {
                    try
                    {
                        int read = await output.ReadAsync(buffer, 0, bufferSize, cancellation);
                        if(read == 0)
                        {
                            exit = true;
                            break;
                        }

                        await discord.WriteAsync(buffer, 0, read, cancellation);

                        if(ServerList.IsPlaying == false)
                        {
                            do
                            {
                                //Do nothing, wait till isplaying is true
                                await Task.Delay(100);
                            } while (ServerList.IsPlaying == false);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        await channel.SendMessageAsync("Song finished");
                        exit = true;
                    }
                    catch
                    {
                        await channel.SendMessageAsync("Sorry an error occured");
                        fail = true;
                    }
                }
                //End
                await discord.FlushAsync();
            }
        }  
    }

    public async Task PauseAudio(IGuild guild, IMessageChannel channel)
    {
        if (guild == null) return; //Check guild if null

        var musicList = GetMusicList(guild.Id);
        if (musicList == null) return; //Check server list if it is null

        musicList.IsPlaying = !musicList.IsPlaying; //Toggel pause status

        if (musicList.IsPlaying) await channel.SendMessageAsync("Current song has been un-paused");
        else await channel.SendMessageAsync("Current song has been paused");

    }

    private string SearchAudio(string search)
    {
        if (!Directory.Exists("Music/")) Directory.CreateDirectory("Music/");

        DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo("Music/");
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