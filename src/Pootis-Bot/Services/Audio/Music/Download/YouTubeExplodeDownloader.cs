using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Helpers;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Pootis_Bot.Services.Audio.Music.Download
{
	public class YouTubeExplodeDownloader : IMusicDownloader
	{
		private readonly YoutubeClient ytClient;
		private readonly string musicDirectory;

		public YouTubeExplodeDownloader(string musicDir = "Music/", HttpClient httpClient = default)
		{
			ytClient = new YoutubeClient(httpClient);
			musicDirectory = musicDir;
		}

		public async Task<string> DownloadYouTubeVideo(string youTubeVideoId, string downloadDirectory = "Music/")
		{
			//Get the video
			Video videoData = await ytClient.Videos.GetAsync(youTubeVideoId);

			try
			{
				//Get the audio stream info
				StreamManifest steamManifest = await ytClient.Videos.Streams.GetManifestAsync(youTubeVideoId);
				IStreamInfo audioSteam = steamManifest.GetAudioOnly().WithHighestBitrate();

				string downloadLocation =
					$"{musicDirectory}{videoData.Title.RemoveIllegalChars()}.{audioSteam.Container.Name}";

				await ytClient.Videos.Streams.DownloadAsync(audioSteam, downloadLocation);

				Logger.Log($"Downloaded song to {downloadLocation}", LogVerbosity.Debug);

				return !File.Exists(downloadLocation) ? null : downloadLocation;
			}
			catch (Exception ex)
			{
#if DEBUG
				Logger.Log(ex.ToString(), LogVerbosity.Error);
#else
				Logger.Log(ex.Message, LogVerbosity.Error);
#endif
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