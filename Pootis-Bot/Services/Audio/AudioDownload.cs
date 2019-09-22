using System;
using System.IO;
using System.Web;
using System.Diagnostics;
using Discord;
using Google.Apis.YouTube.v3.Data;
using Pootis_Bot.Modules.Fun;
using Pootis_Bot.Services.Google;

namespace Pootis_Bot.Services.Audio
{
	public class AudioDownload
	{
		private readonly string _pythonDir = Directory.GetCurrentDirectory() + "/External/python.exe";

		public string DownloadAudio(string search, IMessageChannel channel)
		{
			channel.SendMessageAsync($"Searching youtube for '{search}'");

			SearchListResponse searchListResponse = YoutubeService.Search(search, this.GetType().ToString(), 10);

			if (searchListResponse.Items.Count != 0)
			{
				try
				{
					string videoUrl = FunCmdsConfig.ytStartLink + searchListResponse.Items[0].Id.VideoId;
					string videoTitle = HttpUtility.HtmlDecode(searchListResponse.Items[0].Snippet.Title);
					string videoLoc = "Music/" + videoTitle + ".mp3";

					if (AudioService.SearchAudio(videoTitle) == videoTitle)
						return videoLoc;

					channel.SendMessageAsync($":musical_note: Downloading **{videoTitle}** from **{searchListResponse.Items[0].Snippet.ChannelTitle}**");

					//Use Youtube-dl to download the song and convert it to a .mp3
					CreateYTDLProcess(videoUrl);
					return videoLoc;
				}
				catch (Exception ex)
				{
					channel.SendMessageAsync("Sorry but an error occured. Here are the details:\n" + ex.Message);
					return null;
				}
			}
			else
			{
				channel.SendMessageAsync($"No result for '{search}' were found on YouTube, try typing in something diffrent.");
				return null;
			}
		}

		private void CreateYTDLProcess(string url)
		{
			ProcessStartInfo startinfo = new ProcessStartInfo
			{
				FileName = _pythonDir,
				Arguments = $" ./External/youtube_dl/__main__.py -x --audio-format mp3 -o /music/%(title)s.%(ext)s \"{url}\"",
				CreateNoWindow = false,
				RedirectStandardOutput = false,
				UseShellExecute = true
			};

			Process proc = new Process
			{
				StartInfo = startinfo
			};

			proc.Start();
			proc.WaitForExit();

			proc.Dispose();
		}
	}
}
