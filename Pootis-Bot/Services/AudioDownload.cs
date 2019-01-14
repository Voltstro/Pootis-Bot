using System;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Discord;
using Pootis_Bot;

public class AudioDownload
{
    readonly string ytstartLink = "https://www.youtube.com/watch?v=";
    readonly string youtubedlloc = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/External/youtube-dl.exe";

    public string DownloadAudio(string search, IMessageChannel channel)
    {
        channel.SendMessageAsync($"Searching youtube for '{search}'");

        var youtube = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = Config.bot.apis.apiYoutubeKey,
            ApplicationName = this.GetType().ToString()
        });

        var searchListRequest = youtube.Search.List("snippet");
        searchListRequest.Q = search; // Replace with your search term.
        searchListRequest.MaxResults = 10;

        // Call the search.list method to retrieve results matching the specified query term.
        var searchListResponse = searchListRequest.Execute();

        List<string> videos = new List<string>();

        // Add each result to the appropriate list, and then display the lists of
        // matching videos and channels.
        foreach (var searchResult in searchListResponse.Items)
        {
            switch (searchResult.Id.Kind)
            {
                case "youtube#video":
                    videos.Add(String.Format($"{ytstartLink + searchResult.Id.VideoId}"));
                    break;
            }
        }

        string[] videosarray = videos.ToArray();
        if (videosarray.Length != 0)
        {
            try
            {
                channel.SendMessageAsync($"Downloading {videosarray[0]}");
                CreateYTDLProcess(videosarray[0]);
                channel.SendMessageAsync($"Done");
                return videosarray[0];
            }
            catch (Exception ex)
            {
                channel.SendMessageAsync("An error occured. Here are the detailes:\n" + ex.Message);
                return null;
            }
        }

        return null;
    }

    private void CreateYTDLProcess(string url)
    {
        ProcessStartInfo startinfo = new ProcessStartInfo
        {
            FileName = youtubedlloc,
            Arguments = $"-x --audio-format mp3 -o /music/%(title)s.%(ext)s \"{url}\"",
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            UseShellExecute = true
        };

        Process proc = new Process()
        {
            StartInfo = startinfo
        };

        proc.Start();
        proc.WaitForExit();

        proc.Dispose();
    }
}