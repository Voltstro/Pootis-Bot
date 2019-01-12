using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Pootis_Bot.Entities;

public class AudioService
{
    private readonly string ffmpegloc = $"\\external\ffmpeg.exe";

    private static ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

    public async Task JoinAudio(IGuild guild, IVoiceChannel target)
    {
        var audio = await target.ConnectAsync();

        if (ConnectedChannels.TryAdd(guild.Id, audio))
        {
            //If you add a method to log happenings from this service,
            // you can uncomment these commented lines to make use of that.
            //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
        }
    }

    public async Task LeaveAudio(IGuild guild)
    {
        if (guild == null) return;

        if (ConnectedChannels.TryRemove(guild.Id, out var client))
        {
            await client.StopAsync();
            return;
        }
    }

    public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
    {
        var searchResults = SearchAudio(path);
        if(searchResults == null)
        {
            AudioDownload download = new AudioDownload();
            string results = download.DownloadAudio(path, channel);
            if (results == null) return;
            searchResults = results;
        }
        Console.WriteLine(searchResults);

        if (ConnectedChannels.TryGetValue(guild.Id, out IAudioClient client))
        {
            //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
            using (var ffmpeg = CreateProcess(searchResults))
            using (var stream = client.CreatePCMStream(AudioApplication.Music))
            {
                try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                finally { await stream.FlushAsync(); }       
            }      
        }
    }

    private string SearchAudio(string search)
    {
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
}