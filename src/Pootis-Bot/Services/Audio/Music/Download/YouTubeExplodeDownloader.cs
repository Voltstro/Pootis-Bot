using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Helpers;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Pootis_Bot.Services.Audio.Music.Download
{
	/// <summary>
	/// Downloads YouTube videos using the YouTubeExplode library
	/// </summary>
	public class YouTubeExplodeDownloader : IMusicDownloader
	{
		private readonly CancellationToken cancellationToken;
		private readonly string musicDirectory;
		private readonly YoutubeClient ytClient;

		public YouTubeExplodeDownloader(string musicDir = "Music/", HttpClient httpClient = default,
			CancellationToken cancelToken = default)
		{
			ytClient = new YoutubeClient(httpClient);
			musicDirectory = musicDir;
			cancellationToken = cancelToken;
		}

		public async Task<string> DownloadYouTubeVideo(string youTubeVideoId, string downloadDirectory = "Music/")
		{
			//Get the video
			Video videoData = await ytClient.Videos.GetAsync(youTubeVideoId);

			try
			{
				if (cancellationToken.IsCancellationRequested)
					return null;

				//Get the audio stream info
				StreamManifest steamManifest = await ytClient.Videos.Streams.GetManifestAsync(youTubeVideoId);
				IStreamInfo audioSteam = steamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
				
				string downloadLocation =
					$"{musicDirectory}{videoData.Title.RemoveIllegalChars()}.{audioSteam.Container.Name}";

				Logger.Debug("Downloading YouTube video {@VideoTitle}({@VideoID}) to {@DownloadLocation}", videoData.Title, videoData.Id.Value, downloadLocation);

				await ytClient.Videos.Streams.DownloadAsync(audioSteam, downloadLocation, null, cancellationToken);

				return !File.Exists(downloadLocation) ? null : downloadLocation;
			}
			catch (OperationCanceledException)
			{
				//User cancelled
				return null;
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while download a YouTube video! {@Exception}", ex);

				return null;
			}
		}

		public async Task<string> GetYouTubeVideo(string youTubeVideoId)
		{
			try
			{
				Video video = await ytClient.Videos.GetAsync(youTubeVideoId);
				return video.Title;
			}
			catch (ArgumentException)
			{
				//The video doesn't exist
				return null;
			}
		}
	}
}