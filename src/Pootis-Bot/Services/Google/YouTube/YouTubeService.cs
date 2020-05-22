using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Pootis_Bot.Core.Logging;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace Pootis_Bot.Services.Google.YouTube
{
	public class YouTubeService : IYouTubeSearcher
	{
		private readonly YoutubeClient ytClient;

		public YouTubeService(HttpClient httpClient)
		{
			ytClient = new YoutubeClient(httpClient);
		}

		public async Task<IList<YouTubeVideo>> SearchForYouTube(string search)
		{
			try
			{
				IReadOnlyList<Video> response = await ytClient.Search.GetVideosAsync(search).BufferAsync(5);

				//Create a new list
				IList<YouTubeVideo> videos = new List<YouTubeVideo>(response.Count);
				foreach (Video result in response)
					videos.Add(new YouTubeVideo
					{
						VideoId = result.Id.Value,
						VideoTitle = result.Title,
						VideoAuthor = result.Author,
						VideoDescription = result.Description,
						VideoDuration = result.Duration
					});

				return videos;
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

		public async Task<YouTubeVideo> GetVideo(string videoId)
		{
			Video video = await ytClient.Videos.GetAsync(videoId);
			if (video == null)
				return null;

			return new YouTubeVideo
			{
				VideoId = videoId,
				VideoDuration = video.Duration,
				VideoAuthor = video.Author,
				VideoTitle = video.Title
			};
		}
	}
}