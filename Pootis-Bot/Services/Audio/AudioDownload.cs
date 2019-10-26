using System;
using System.Diagnostics;
using System.IO;
using Discord;
using Google.Apis.YouTube.v3.Data;
using Pootis_Bot.Modules.Fun;
using Pootis_Bot.Services.Google;

namespace Pootis_Bot.Services.Audio
{
	public class AudioDownload
	{
		private readonly string _pythonDir = Directory.GetCurrentDirectory() + "/External/python.exe";

		/// <summary>
		/// Downloads an audio file using a search string
		/// </summary>
		/// <param name="search">The string to search for</param>
		/// <param name="channel">What channel are we on?</param>
		/// <returns></returns>
		public string DownloadAudio(string search, IMessageChannel channel)
		{
			channel.SendMessageAsync($"Searching youtube for '{search}'");

			SearchListResponse searchListResponse = YoutubeService.Search(search, GetType().ToString());

			if (searchListResponse.Items.Count != 0)
				try
				{
					string videoUrl = FunCmdsConfig.ytStartLink + searchListResponse.Items[0].Id.VideoId;
					string videoTitle = AudioCheckService.RemovedNotAllowedChars(searchListResponse.Items[0].Snippet.Title);
					string videoLoc = $"Music/{videoTitle}.mp3";

					//Do a second check to see if we have already have that video
					string check = AudioService.SearchAudio(videoTitle);
					if (!string.IsNullOrWhiteSpace(check)) return videoLoc;

					channel.SendMessageAsync(
						$":musical_note: Downloading **{videoTitle}** from **{searchListResponse.Items[0].Snippet.ChannelTitle}**");

					//Use Youtube-dl to download the song and convert it to a .mp3
					CreateYtdlProcess(videoUrl, videoTitle);
					return videoLoc;
				}
				catch (Exception ex)
				{
					channel.SendMessageAsync("Sorry but an error occured. Here are the details:\n" + ex.Message);
					return null;
				}

			channel.SendMessageAsync(
				$"No result for '{search}' were found on YouTube, try typing in something different.");
			return null;
		}

		private void CreateYtdlProcess(string url, string name)
		{
			ProcessStartInfo startinfo = new ProcessStartInfo
			{
				FileName = _pythonDir,
				Arguments =
					$" ./External/youtube_dl/__main__.py -x --audio-format mp3 -o /music/\"{name}\".%(ext)s \"{url}\"",
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