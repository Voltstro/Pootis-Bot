using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Discord;
using Pootis_Bot;

public class AudioDownload
{
    readonly string ytstartLink = "https://www.youtube.com/watch?v=";

    public string DownloadAudio(string search, IMessageChannel channel)
    {
        var result = SearchYoutube(search);
        if (result != "fail")
        {
            channel.SendMessageAsync($"Download of '{search}' was successfull");
            return result;
        }
        else
        {
            channel.SendMessageAsync($"Download of '{search}' failed");
            return "fail";
        }
    }

    public string SearchYoutube(string search)
    {
        Console.WriteLine("Searching youtube for " + search);
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
        if(videosarray.Length != 0)
        {
            try
            {
                CreateYTDLProcess(videosarray[0]);
                return videosarray[0];
            }
            catch
            {
                return "fail";
            }       
        }
        Console.WriteLine("Searching failed");

        return "fail";

    }

    private async Task CreateYTDLProcess(string url)
    {
        Console.WriteLine("Starting download of " + url);
        try
        {
            ProcessStartInfo startinfo = new ProcessStartInfo
            {
                FileName = "youtube-dl.exe",
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
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

//youtube-dl -x --audio-format mp3 -o "\g\%(title)s.%(ext)s" https://www.youtube.com/watch?v=ZcoqR9Bwx1Y