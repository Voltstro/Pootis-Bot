using System;
using System.Collections.Concurrent;
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
    private readonly string ffmpegloc = $"\\external\ffmpeg.exe";

    private static List<GlobalServerMusicItem> channels = new List<GlobalServerMusicItem>();

    public async Task JoinAudio(IGuild guild, IVoiceChannel target)
    {
        var audio = await target.ConnectAsync();

        var item = new GlobalServerMusicItem
        {
            GuildID = guild.Id,
            IsPlaying = false,
            AudioClient = audio
        };

        channels.Add(item);
    }

    public async Task LeaveAudio(IGuild guild)
    {
        if (guild == null) return;

        await GetMusicList(guild.Id).AudioClient.StopAsync();
        GetMusicList(guild.Id).IsPlaying = false;
    }

    public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
    {
        var searchResults = SearchAudio(path);

        Console.WriteLine(searchResults);
        if(searchResults == null)
        {
            AudioDownload download = new AudioDownload();
            string results = download.DownloadAudio(path, channel);
            if (results == null) return;
            searchResults = results;
        }

        Console.WriteLine(searchResults);

        var client = GetMusicList(guild.Id).AudioClient;
    
        //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
        using (var ffmpeg = CreateProcess(searchResults))
        using (var stream = client.CreatePCMStream(AudioApplication.Music))
        {
           try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
           finally { await stream.FlushAsync(); }       
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
            FileName = "external/ffmpeg.exe",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
    }

    #region List Fuctions

    private GlobalServerMusicItem GetMusicList(ulong guildid)
    {
        var result = from a in channels
                     where a.GuildID == guildid
                     select a;

        var list = result.FirstOrDefault();
        return list;
    }

    #endregion
}