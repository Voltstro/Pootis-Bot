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

namespace Pootis_Bot.Services.Audio
{
	public class AudioDownloadMusicFiles : IDisposable
	{
		private readonly YoutubeClient _client = new YoutubeClient(Global.HttpClient);
		
		private readonly CancellationTokenSource _cancellationSource;
		private readonly CancellationToken _downloadCancellationToken;

		private readonly IUserMessage _message;
		private readonly SocketGuild _guild;
		private readonly TimeSpan _maxAudioTime;
		private readonly string _downloadLocation;
		private readonly string _downloadFileContainer;

		private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);
		private bool _disposed;

		private bool _hasFinishedDownloading;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_cancellationSource.Cancel();
			if(!_hasFinishedDownloading)
				_message.DeleteAsync().GetAwaiter().GetResult();

			if (disposing)
				_handle.Dispose();

			_disposed = true;
		}

		public AudioDownloadMusicFiles(IUserMessage message, SocketGuild guild, TimeSpan maxVideoTime, string downloadLocation = "Music/", string audioFileContainer = "mp3")
		{
			//Setup our cancellation token
			_cancellationSource = new CancellationTokenSource();
			_downloadCancellationToken = _cancellationSource.Token;

			_message = message;
			_guild = guild;
			_maxAudioTime = maxVideoTime;
			_downloadLocation = downloadLocation;
			_downloadFileContainer = audioFileContainer;
		}

		public string DownloadAudioByTitle(string search)
		{
			_hasFinishedDownloading = false;

			IList<SearchResult> youTubeSearchVideoResults = YoutubeService.Search(search, GetType().ToString(), 10, "video").Items;
			if (youTubeSearchVideoResults.Count != 0)
				return DownloadAudioById(youTubeSearchVideoResults.FirstOrDefault()?.Id.VideoId);

			MessageUtils.ModifyMessage(_message, $":musical_note: The search `{search}` didn't find anything, try searching for something different.").GetAwaiter().GetResult();
			return null;

		}

		public string DownloadAudioById(string videoId)
		{
			_hasFinishedDownloading = false;

			Video youTubeVideo;
			try
			{
				youTubeVideo = _client.GetVideoAsync(videoId).GetAwaiter().GetResult();
			}
			catch (ArgumentException)
			{
				MessageUtils.ModifyMessage(_message, ":musical_note: That YouTube URL is invalid!").GetAwaiter().GetResult();
				_hasFinishedDownloading = true;
				return null;
			}

			//Do a search in our music directory using the EXACT title
			string searchResult = AudioService.SearchAudio(AudioCheckService.RemovedNotAllowedChars(youTubeVideo.Title));
			if (!string.IsNullOrWhiteSpace(searchResult))
			{
				_hasFinishedDownloading =
					true; //Set this to true anyway, so when disposing it doesn't delete the message

				return searchResult;
			}

			if (youTubeVideo.Duration <= _maxAudioTime) return DownloadAudio(youTubeVideo);

			MessageUtils.ModifyMessage(_message, $":musical_note: Video succeeds max time of {_maxAudioTime}").GetAwaiter().GetResult();
			return null;
		}

		private string DownloadAudio(Video youTubeVideo)
		{
			try
			{
				//Make sure we haven't been canceled yet
				if (_cancellationSource.IsCancellationRequested)
					return null;

				string videoTitle = AudioCheckService.RemovedNotAllowedChars(youTubeVideo.Title);

				MessageUtils.ModifyMessage(_message,
						$":musical_note: Give me a sec. Downloading **{youTubeVideo.Title}** from **{youTubeVideo.Author}**...")
					.GetAwaiter().GetResult();

				//Get our video stream info
				MediaStreamInfoSet videoMediaInfo =
					_client.GetVideoMediaStreamInfosAsync(youTubeVideo.Id).GetAwaiter().GetResult();
				AudioStreamInfo streamInfo = videoMediaInfo.Audio.WithHighestBitrate();
				string downloadLocation = $"{_downloadLocation}{videoTitle}.{streamInfo.Container.GetFileExtension()}";

				//Download the audio file
				_client.DownloadMediaStreamAsync(streamInfo, downloadLocation, null, _downloadCancellationToken)
					.GetAwaiter().GetResult();

				//Do another check to make sure our video hasn't been canceled
				if (_cancellationSource.IsCancellationRequested)
					return null;

				Logger.Log($"The downloaded video file extension is '{streamInfo.Container.GetFileExtension()}'.", LogVerbosity.Debug);
				if (streamInfo.Container.GetFileExtension() != _downloadFileContainer)
				{
					if(!ConvertAudioFileToMp3(downloadLocation,
						$"{_downloadLocation}{videoTitle}.{_downloadFileContainer}"))
					{
						MessageUtils.ModifyMessage(_message, "Sorry, but there was an issue downloading the song! Try again later.").GetAwaiter().GetResult();
						return null;
					}
				}

				downloadLocation = $"{_downloadLocation}{videoTitle}.{_downloadFileContainer}";
				_hasFinishedDownloading = true; 

				//We have finished downloading
				return downloadLocation;
			}
			catch (Exception ex)
			{
				Logger.Log(ex.Message, LogVerbosity.Error);

				MessageUtils
					.ModifyMessage(_message, "Sorry, but there was an issue downloading the song! Try again later.")
					.GetAwaiter().GetResult();

				//Log out an error to the owner if they have it enabled
				if (Config.bot.ReportErrorsToOwner)
					Global.BotOwner.SendMessageAsync(
						$"ERROR: {ex.Message}\nError occured while trying to search or download a video from YouTube on guild `{_guild.Id}`.");

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
				if (_downloadCancellationToken.IsCancellationRequested)
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
			if(File.Exists(fileToConvert))
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