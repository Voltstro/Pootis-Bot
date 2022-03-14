using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Pootis_Bot.Core.Logging;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
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
				IReadOnlyList<VideoSearchResult> response = await ytClient.Search.GetVideosAsync(search);

				//Create a new list
				IList<YouTubeVideo> videos = new List<YouTubeVideo>(response.Count);
				foreach (VideoSearchResult result in response)
					videos.Add(new YouTubeVideo
					{
						VideoId = result.Id.Value,
						VideoTitle = result.Title,
						VideoAuthor = result.Author.Title,
						VideoDescription = "YouTube broke descriptions :( I am too lazy to bother fixing anything.",
						VideoDuration = result.Duration.GetValueOrDefault()
					});

				return videos;
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while searching YouTube! {@Exception}", ex);
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
				VideoDuration = video.Duration.GetValueOrDefault(),
				VideoAuthor = video.Author.Title,
				VideoTitle = video.Title
			};
		}
	}
}