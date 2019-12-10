using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Discord;
using Discord.WebSocket;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Win32.SafeHandles;
using Pootis_Bot.Core;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services.Google;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace Pootis_Bot.Services.Audio
{
	public class AudioDownloadMusicFiles
	{
		private readonly YoutubeClient _client = new YoutubeClient(Global.HttpClient);

		private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
				_handle.Dispose();

			_disposed = true;
		}

		/// <summary>
		/// Downloads an audio file using a search string
		/// </summary>
		/// <param name="search">The string to search for</param>
		/// <param name="message">The base message</param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public string DownloadAudio(string search, IUserMessage message, SocketGuild guild)
		{
			MessageUtils.ModifyMessage(message, $":musical_note: Searching YouTube for '{search}'").GetAwaiter()
				.GetResult();

			SearchListResponse searchListResponse = YoutubeService.Search(search, GetType().ToString());

			if (searchListResponse.Items.Count != 0)
				try
				{
					//Get video details first
					MediaStreamInfoSet videoInfo = _client
						.GetVideoMediaStreamInfosAsync(searchListResponse.Items[0].Id.VideoId).GetAwaiter().GetResult();
					string videoTitle =
						AudioCheckService.RemovedNotAllowedChars(searchListResponse.Items[0].Snippet.Title);
					string videoLoc = $"Music/{videoTitle}.mp3";

					//Do a second check to see if we have already have that video
					string check = AudioService.SearchAudio(videoTitle);
					if (!string.IsNullOrWhiteSpace(check)) return videoLoc;

					//Get the video time
					TimeSpan videoTime = _client.GetVideoAsync(searchListResponse.Items[0].Id.VideoId).GetAwaiter()
						.GetResult().Duration;

					//Check to make sure the video doesn't succeeds the max video time
					if (videoTime.TotalSeconds > Config.bot.AudioSettings.MaxVideoTime.TotalSeconds)
					{
						MessageUtils.ModifyMessage(message,
								$":musical_note: Video succeeds max time of {Config.bot.AudioSettings.MaxVideoTime}")
							.GetAwaiter().GetResult();

						return null;
					}

					Debug.WriteLine($"[Audio Download] Downloading {videoLoc}");

					MessageUtils.ModifyMessage(message,
							$":musical_note: Give me a sec. Downloading **{videoTitle}** from **{searchListResponse.Items[0].Snippet.ChannelTitle}**...")
						.GetAwaiter().GetResult();

					//Download the .mp3 file
					_client.DownloadMediaStreamAsync(videoInfo.Audio.WithHighestBitrate(), videoLoc).GetAwaiter()
						.GetResult();

					Debug.WriteLine("[Audio Download] Download successful");

					return videoLoc;
				}
				catch (Exception ex)
				{
					Global.Log(ex.Message, ConsoleColor.Red);
					MessageUtils
						.ModifyMessage(message, $"Sorry but an error occured. Here are the details:\n{ex.Message}")
						.GetAwaiter().GetResult();

					//Log out an error to the owner if they have it enabled
					if (Config.bot.ReportErrorsToOwner)
						Global.BotOwner.SendMessageAsync(
							$"ERROR: {ex.Message}\nError occured while trying to search or download a video from YouTube on server `{guild.Id}`.");

					return null;
				}

			MessageUtils
				.ModifyMessage(message,
					$":musical_note: No result for '{search}' were found on YouTube, try typing in something different.")
				.GetAwaiter().GetResult();

			return null;
		}
	}
}