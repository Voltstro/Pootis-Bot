using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Google.Apis.YouTube.v3.Data;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services.Audio.Music.Conversion;
using Pootis_Bot.Services.Google;
using YoutubeExplode;
using Video = YoutubeExplode.Videos.Video;

namespace Pootis_Bot.Services.Audio.Music.Download
{
	//TODO: Add cancellation download support

	public class StandardMusicDownloader
	{
		private readonly string musicDirectory;
		private readonly MusicFileFormat fileFormat;

		//Interfaces
		private readonly IAudioConverter audioConverter;		//Default: FfmpegAudioConverter
		private readonly IMusicDownloader musicDownloader;		//Default: YouTubeExplodeDownloader

		//TODO: We won't need YoutubeClient in this class once I write a 'YouTube searcher' interface
		private readonly YoutubeClient ytClient;

		public StandardMusicDownloader(string musicDir, MusicFileFormat musicFileFormat, HttpClient httpClient)
		{
			if (!Directory.Exists(musicDir))
				Directory.CreateDirectory(musicDir);

			audioConverter = new FfmpegAudioConverter();
			musicDownloader = new YouTubeExplodeDownloader(musicDir, httpClient);

			musicDirectory = musicDir;
			fileFormat = musicFileFormat;
			ytClient = new YoutubeClient(httpClient);
		}

		public async Task<string> GetSongViaYouTubeUrl(string videoUrl, IUserMessage botMessage)
		{
			Video video = await ytClient.Videos.GetAsync(videoUrl);
			if (video != null)
			{
				return await GetOrDownloadSong(video.Title, botMessage);
			}

			await MessageUtils.ModifyMessage(botMessage, "Parsed in URL is incorrect or the YouTube video doesn't exist!");
			return null;
		}

		public async Task<string> GetOrDownloadSong(string songTitle, IUserMessage botMessage)
		{
			try
			{
				await MessageUtils.ModifyMessage(botMessage, $"Searching my audio banks for '{songTitle}'");

				//First, check if this song exists in our music DIR
				string songLocation = AudioService.SearchMusicDirectory(songTitle, fileFormat);
				if (songLocation != null)
				{
					return songLocation;
				}

				await MessageUtils.ModifyMessage(botMessage, $"Searching YouTube for '{songTitle}'");

				//It doesn't exist, search YouTube for it
				SearchListResponse response = YoutubeService.Search(songTitle, GetType().ToString(), 10, "video");

				if (response == null)
				{
					await MessageUtils.ModifyMessage(botMessage, "Something went wrong while searching on YouTube!");
					return null;
				}

				//There were no results
				if (response.Items.Count == 0)
				{
					await MessageUtils.ModifyMessage(botMessage,
						$"There were no results for '{songTitle}' on YouTube.");
					return null;
				}

				//Get the first video
				Video video = await ytClient.Videos.GetAsync(response.Items[0].Id.VideoId);

				//This shouldn't ever happen
				if (video == null)
				{
					await MessageUtils.ModifyMessage(botMessage,
						$"Some issue happened while getting '{songTitle}' off from YouTube.");
					return null;
				}

				string videoTitle = video.Title.RemoveIllegalChars();

				//Do a second search with the title from YouTube
				songLocation = AudioService.SearchMusicDirectory(videoTitle, fileFormat);
				if (songLocation != null)
				{
					return songLocation;
				}

				//Make sure the song doesn't succeeds max time
				if (video.Duration >= Config.bot.AudioSettings.MaxVideoTime)
				{
					Logger.Log(video.Duration.ToString());
					await MessageUtils.ModifyMessage(botMessage,
						$"The video **{videoTitle}** by **{video.Author}** succeeds max time of {Config.bot.AudioSettings.MaxVideoTime}");
					return null;
				}

				//Download the song
				await MessageUtils.ModifyMessage(botMessage, $"Downloading **{videoTitle}** by **{video.Author}**");
				songLocation = await musicDownloader.DownloadYouTubeVideo(video.Id, musicDirectory);

				//The download must have failed
				if (songLocation == null)
				{
					await MessageUtils.ModifyMessage(botMessage,
						$"Something went wrong while downloading the song **{videoTitle}** from YouTube!");
					return null;
				}

				//If the file extension isn't the same then we need to convert it
				string audioFileExtension = Path.GetExtension(songLocation);
				if (audioFileExtension == fileFormat.GetFormatExtension()) return songLocation;

				//We need to convert it, since they are not the same file format
				songLocation = await audioConverter.ConvertFileToAudio(songLocation, musicDirectory, true, fileFormat);

				//Everything when well
				if (songLocation != null) return songLocation;

				//Conversion failed
				await MessageUtils.ModifyMessage(botMessage,
					"An issue occured while getting the song ready for playing!");
				return null;
			}
			catch (Exception ex)
			{
#if DEBUG
				Logger.Log(ex.ToString(), LogVerbosity.Error);
#else
				Logger.Log(ex.Message, LogVerbosity.Error);
#endif
				await MessageUtils.ModifyMessage(botMessage, "An issue occured while trying to get the song!");

				return null;
			}
		}
	}
}