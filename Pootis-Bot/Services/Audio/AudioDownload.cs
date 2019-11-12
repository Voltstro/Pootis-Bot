using System;
using System.Diagnostics;
using System.IO;
using Discord;
using Discord.WebSocket;
using Google.Apis.YouTube.v3.Data;
using Pootis_Bot.Core;
using Pootis_Bot.Services.Google;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace Pootis_Bot.Services.Audio
{
	public class AudioDownload
	{
		private readonly YoutubeClient _client = new YoutubeClient();

		/// <summary>
		/// Downloads an audio file using a search string
		/// </summary>
		/// <param name="search">The string to search for</param>
		/// <param name="channel">What channel are we on?</param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public string DownloadAudio(string search, IMessageChannel channel, SocketGuild guild)
		{
			channel.SendMessageAsync($"Searching YouTube for '{search}'");

			SearchListResponse searchListResponse = YoutubeService.Search(search, GetType().ToString());

			if (searchListResponse.Items.Count != 0)
			{
				try
				{
					MediaStreamInfoSet videoInfo = _client.GetVideoMediaStreamInfosAsync(searchListResponse.Items[0].Id.VideoId).GetAwaiter().GetResult();
					
					string videoTitle = AudioCheckService.RemovedNotAllowedChars(searchListResponse.Items[0].Snippet.Title);
					string videoLoc = $"Music/{videoTitle}.mp3";

					//Do a second check to see if we have already have that video
					string check = AudioService.SearchAudio(videoTitle);
					if (!string.IsNullOrWhiteSpace(check)) return videoLoc;

					//Check to make sure the music directory is there
					if (!Directory.Exists("Music/")) Directory.CreateDirectory("Music/");

					TimeSpan videoTime = _client.GetVideoAsync(searchListResponse.Items[0].Id.VideoId).GetAwaiter()
						.GetResult().Duration;

					if (videoTime.TotalSeconds > Config.bot.AudioSettings.MaxVideoTime.TotalSeconds)
					{
						channel.SendMessageAsync($":musical_note: Video succeeds max time of {Config.bot.AudioSettings.MaxVideoTime}").GetAwaiter().GetResult();

						return null;
					}

					Debug.WriteLine($"[Audio Download] Downloading {videoLoc}");

					channel.SendMessageAsync(
						$":musical_note: Downloading **{videoTitle}** from **{searchListResponse.Items[0].Snippet.ChannelTitle}**");

					//Download the .mp3 file
					_client.DownloadMediaStreamAsync(videoInfo.Audio.WithHighestBitrate(), videoLoc).GetAwaiter().GetResult();

					Debug.WriteLine("[Audio Download] Download successful");

					return videoLoc;
				}
				catch (Exception ex)
				{
					Global.Log(ex.Message, ConsoleColor.Red);
					channel.SendMessageAsync("Sorry but an error occured. Here are the details:\n" + ex.Message);

					//Log out an error to the owner if they have it enabled
					if (Config.bot.ReportErrorsToOwner)
						Global.BotOwner.SendMessageAsync(
							$"ERROR: {ex.Message}\nError occured while trying to search or download a video from YouTube on server `{guild.Id}`.");

					return null;
				}
			}

			channel.SendMessageAsync(
				$"No result for '{search}' were found on YouTube, try typing in something different.");
			return null;
		}
	}
}