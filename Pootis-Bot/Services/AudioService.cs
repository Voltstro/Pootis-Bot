using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        var searchResults = SearchAudio(search); //Search to see if we might allready have the song
        string results;

        if(searchResults == null)
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



        var client = GetMusicList(guild.Id).AudioClient;
    
        using (var ffmpeg = CreateProcess(results))
        using (var stream = client.CreatePCMStream(AudioApplication.Music))
        {
           await channel.SendMessageAsync($"Now playing '{search}'");
           try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
           finally { await stream.FlushAsync(); ffmpeg.Dispose(); }       
        }      
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

    private Process CreateProcess(string path)
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