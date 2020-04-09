using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Discord;
using Discord.WebSocket;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Win32.SafeHandles;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services.Google;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using Video = YoutubeExplode.Models.Video;

namespace Pootis_Bot.Services.Audio.Music
{
	public class MusicDownloader : IDisposable
	{
		private readonly CancellationTokenSource cancellationSource;
		private readonly YoutubeClient client = new YoutubeClient(Global.HttpClient);
		private readonly CancellationToken downloadCancellationToken;
		private readonly MusicFileFormat downloadFileContainer;
		private readonly string downloadLocation;
		private readonly SocketGuild guild;
		private readonly TimeSpan maxAudioTime;
		private readonly IUserMessage message;

		private readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
		private bool disposed;

		private bool hasFinishedDownloading;

		/// <summary>
		/// A class for downloading audio files so that we can play them with the <see cref="AudioService"/>
		/// </summary>
		/// <param name="message">The 'base' message that we will modify over time to tell the user what we are up to</param>
		/// <param name="guild">The guild that the command was executed in</param>
		/// <param name="maxVideoTime">The max video length we will download</param>
		/// <param name="downloadLocation">The place to download to</param>
		/// <param name="audioFileContainer">The audio files container</param>
		public MusicDownloader(IUserMessage message, SocketGuild guild, TimeSpan maxVideoTime,
			string downloadLocation = "Music/", MusicFileFormat audioFileContainer = MusicFileFormat.Mp3)
		{
			//Setup our cancellation token
			cancellationSource = new CancellationTokenSource();
			downloadCancellationToken = cancellationSource.Token;

			this.message = message;
			this.guild = guild;
			maxAudioTime = maxVideoTime;
			this.downloadLocation = downloadLocation;
			downloadFileContainer = audioFileContainer;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			cancellationSource.Cancel();
			if (!hasFinishedDownloading)
				message.DeleteAsync().GetAwaiter().GetResult();

			if (disposing)
				handle.Dispose();

			disposed = true;
		}

		/// <summary>
		/// Downloads and converts a video to an audio file (Searches videos with title)
		/// </summary>
		/// <param name="search">What song we are searching for</param>
		/// <returns></returns>
		public string DownloadAudioByTitle(string search)
		{
			hasFinishedDownloading = false;

			IList<SearchResult> youTubeSearchVideoResults =
				YoutubeService.Search(search, GetType().ToString(), 10, "video").Items;
			if (youTubeSearchVideoResults.Count != 0)
				return DownloadAudioById(youTubeSearchVideoResults.FirstOrDefault()?.Id.VideoId);

			MessageUtils
				.ModifyMessage(message,
					$":musical_note: The search `{search}` didn't find anything, try searching for something different.")
				.GetAwaiter().GetResult();
			return null;
		}

		/// <summary>
		/// Downloads and converts a video to an audio file (Directly with the video ID)
		/// </summary>
		/// <param name="videoId">The video ID</param>
		/// <returns></returns>
		public string DownloadAudioById(string videoId)
		{
			hasFinishedDownloading = false;

			Video youTubeVideo;
			try
			{
				youTubeVideo = client.GetVideoAsync(videoId).GetAwaiter().GetResult();
			}
			catch (ArgumentException)
			{
				MessageUtils.ModifyMessage(message, ":musical_note: That YouTube URL is invalid!").GetAwaiter()
					.GetResult();
				hasFinishedDownloading = true;
				return null;
			}

			//Do a search in our music directory using the EXACT title
			string searchResult =
				AudioService.SearchMusicDirectory(youTubeVideo.Title.RemoveIllegalChars(), downloadFileContainer);
			if (!string.IsNullOrWhiteSpace(searchResult))
			{
				hasFinishedDownloading =
					true; //Set this to true anyway, so when disposing it doesn't delete the message

				return searchResult;
			}

			if (youTubeVideo.Duration <= maxAudioTime) return DownloadAudio(youTubeVideo);

			MessageUtils.ModifyMessage(message, $":musical_note: Video succeeds max time of {maxAudioTime}")
				.GetAwaiter().GetResult();
			return null;
		}

		private string DownloadAudio(Video youTubeVideo)
		{
			try
			{
				//Make sure we haven't been canceled yet
				if (cancellationSource.IsCancellationRequested)
					return null;

				string videoTitle = youTubeVideo.Title.RemoveIllegalChars();

				MessageUtils.ModifyMessage(message,
						$":musical_note: Give me a sec. Downloading **{videoTitle}** from **{youTubeVideo.Author}**...")
					.GetAwaiter().GetResult();

				//Get our video stream info
				MediaStreamInfoSet videoMediaInfo =
					client.GetVideoMediaStreamInfosAsync(youTubeVideo.Id).GetAwaiter().GetResult();
				AudioStreamInfo streamInfo = videoMediaInfo.Audio.WithHighestBitrate();
				string songDownloadLocation = $"{downloadLocation}{videoTitle}.{streamInfo.Container.GetFileExtension()}";

				//Download the audio file
				client.DownloadMediaStreamAsync(streamInfo, songDownloadLocation, null, downloadCancellationToken)
					.GetAwaiter().GetResult();

				//Do another check to make sure our video hasn't been canceled
				if (cancellationSource.IsCancellationRequested)
					return null;

				Logger.Log($"The downloaded video file extension is '{streamInfo.Container.GetFileExtension()}'.",
					LogVerbosity.Debug);
				if (streamInfo.Container.GetFileExtension() != downloadFileContainer.GetFormatExtension())
				{
					if (cancellationSource.IsCancellationRequested)
						return null;

					if (!ConvertAudioFileToMp3(songDownloadLocation,
						$"{this.downloadLocation}{videoTitle}.{downloadFileContainer.GetFormatExtension()}"))
					{
						if (!disposed)
							MessageUtils.ModifyMessage(message,
									"Sorry, but there was an issue downloading the song! Try again later.").GetAwaiter()
								.GetResult();
						return null;
					}
				}

				songDownloadLocation = $"{this.downloadLocation}{videoTitle}.{downloadFileContainer.GetFormatExtension()}";
				hasFinishedDownloading = true;

				//We have finished downloading
				return songDownloadLocation;
			}
			catch (Exception ex)
			{
#if DEBUG
				Logger.Log(ex.ToString(), LogVerbosity.Error);
#else
				Logger.Log(ex.Message, LogVerbosity.Error);
#endif

				MessageUtils
					.ModifyMessage(message, "Sorry, but there was an issue downloading the song! Try again later.")
					.GetAwaiter().GetResult();

				//Log out an error to the owner if they have it enabled
				if (Config.bot.ReportErrorsToOwner)
					Global.BotOwner.SendMessageAsync(
						$"ERROR: {ex.Message}\nError occured while trying to search or download a video from YouTube on guild `{guild.Id}`.");

				//Mark this as true so our error doesn't get deleted
				hasFinishedDownloading = true;

				return null;
			}
		}

		private bool ConvertAudioFileToMp3(string fileToConvert, string fileLocation)
		{
			Logger.Log($"Converting '{fileToConvert}' to '{fileLocation}'...", LogVerbosity.Debug);

			//Start our ffmpeg process
			Process ffmpeg = Process.Start(new ProcessStartInfo
			{
				FileName = Config.bot.AudioSettings.FfmpegLocation,
				Arguments = $"-i \"{fileToConvert}\" \"{fileLocation}\"",
				CreateNoWindow = true
			});

			while (ffmpeg != null && !ffmpeg.HasExited)
			{
				if (downloadCancellationToken.IsCancellationRequested)
				{
					//Kill ffmpeg on cancel
					ffmpeg.Kill(true);
					ffmpeg.Dispose();

					//Delete our work in progress files
					File.Delete(fileLocation);
					File.Delete(fileToConvert);

					return false;
				}

				Thread.Sleep(100);
			}

			//Delete our old file
			if (File.Exists(fileToConvert))
				File.Delete(fileToConvert);
			else //Were the fuck is our fileToConvert then?? This should never happen but it is here anyway
				return false;

			//So obviously there was an issue converting...
			if (!File.Exists(fileLocation))
			{
				Logger.Log("There was an issue converting the file!", LogVerbosity.Debug);
				return false;
			}

			//Ayy, we converted
			Logger.Log($"Successfully converted to '{fileLocation}'.", LogVerbosity.Debug);
			return true;
		}
	}
}