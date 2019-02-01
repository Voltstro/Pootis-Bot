using System;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Discord;
using Pootis_Bot;

public class AudioDownload
{
    readonly string ytstartLink = "https://www.youtube.com/watch?v="; //The begining of the yt URL
    readonly string ytstartLinkShort = "https://youtu.be/";
    readonly string youtubedlloc = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/External/youtube-dl";

    public string DownloadAudio(string search, IMessageChannel channel)
    {
        channel.SendMessageAsync($"Searching youtube for '{search}'");

        var youtube = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = Config.bot.apis.apiYoutubeKey,
            ApplicationName = this.GetType().ToString()
        });

        var searchListRequest = youtube.Search.List("snippet");
        searchListRequest.Q = search; 
        searchListRequest.MaxResults = 10;

        // Call the search.list method to retrieve results matching the specified query term.
        var searchListResponse = searchListRequest.Execute();

        if (searchListResponse.Items.Count != 0)
        {
            try
            {
                string videourl = ytstartLink + searchListResponse.Items[0].Id.VideoId;
                string videotitle = searchListResponse.Items[0].Snippet.Title;
                string videoloc = "Music/" + videotitle + ".mp3";

                EmbedBuilder embed = new EmbedBuilder();
                embed.WithTitle(videotitle);
                embed.WithDescription($"Downloading '{videotitle}'\n**Please Wait**");
                embed.AddField("Uploader", searchListResponse.Items[0].Snippet.ChannelTitle);
                embed.AddField("URL", ytstartLinkShort + searchListResponse.Items[0].Id.VideoId);
                embed.WithImageUrl(searchListResponse.Items[0].Snippet.Thumbnails.Medium.Url);
                channel.SendMessageAsync("", false, embed.Build());

                //Use Youtube-dl to download the song and convert it to a .mp3
                CreateYTDLProcess(videourl);
                channel.SendMessageAsync($"Done");
                return videoloc;
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