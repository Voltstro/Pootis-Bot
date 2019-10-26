using System;
using System.Diagnostics;
using System.IO;
using Discord;
using Discord.WebSocket;
using Google.Apis.YouTube.v3.Data;
using Pootis_Bot.Core;
using Pootis_Bot.Modules.Fun;
using Pootis_Bot.Services.Google;

namespace Pootis_Bot.Services.Audio
{
	public class AudioDownload
	{
		/// <summary>
		/// Downloads an audio file using a search string
		/// </summary>
		/// <param name="search">The string to search for</param>
		/// <param name="channel">What channel are we on?</param>
		/// <returns></returns>
		public string DownloadAudio(string search, IMessageChannel channel, SocketGuild guild)
		{
			channel.SendMessageAsync($"Searching YouTube for '{search}'");

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

					if (!Directory.Exists("Music/")) Directory.CreateDirectory("Music/");

					channel.SendMessageAsync(
						$":musical_note: Downloading **{videoTitle}** from **{searchListResponse.Items[0].Snippet.ChannelTitle}**");

					//Use Youtube-dl to download the song and convert it to a .mp3
					CreateYtDownloadProcess(videoUrl, videoTitle);
					return videoLoc;
				}
				catch (Exception ex)
				{
					Global.Log(ex.Message, ConsoleColor.Red);
					channel.SendMessageAsync("Sorry but an error occured. Here are the details:\n" + ex.Message);

					//Log out an error to the owner if they have it enabled
					if (Config.bot.ReportErrorsToOwner)
						channel.SendMessageAsync(
							$"ERROR: {ex.Message}\nError occured while trying to search or download a video from YouTube on server `{guild.Id}`.");

					return null;
				}

			channel.SendMessageAsync(
				$"No result for '{search}' were found on YouTube, try typing in something different.");
			return null;
		}

		private static void CreateYtDownloadProcess(string url, string name)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				FileName = Config.bot.AudioSettings.InitialApplication,
				Arguments =
					$" {Config.bot.AudioSettings.PythonArguments} -x --audio-format mp3 -o ./Music/\"{name}\".%(ext)s \"{url}\"",
				CreateNoWindow = !Config.bot.AudioSettings.ShowYoutubeDlWindow,
				RedirectStandardOutput = false,
				UseShellExecute = Config.bot.AudioSettings.ShowYoutubeDlWindow
			};

			Process proc = new Process
			{
				StartInfo = processStartInfo
			};

			proc.Start();
			proc.WaitForExit();

			proc.Dispose();
		}
	}
}