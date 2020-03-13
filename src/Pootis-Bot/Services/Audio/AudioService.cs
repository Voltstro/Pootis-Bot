using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Services.Audio
{
	public class AudioService
	{
		private const string MusicDir = "Music/";

		public static readonly List<ServerMusicItem> currentChannels = new List<ServerMusicItem>();

		/// <summary>
		/// Joins a voice channel
		/// </summary>
		/// <param name="guild">The <see cref="IGuild"/> where the voice channel is in</param>
		/// <param name="target">The target <see cref="IVoiceChannel"/> to join</param>
		/// <param name="channel">The <see cref="IMessageChannel"/> to log messages to</param>
		/// <param name="user">The <see cref="IUser"/> who requested this command</param>
		/// <returns></returns>
		public async Task JoinAudio(IGuild guild, IVoiceChannel target, IMessageChannel channel, IUser user)
		{
			if (target == null)
			{
				await channel.SendMessageAsync(":musical_note: You need to be in a voice channel!");
				return;
			}

			ServerMusicItem serverMusic = GetMusicList(guild.Id);
			if (serverMusic != null)
			{
				if (serverMusic.AudioChannel.GetUser(user.Id) != null)
				{
					await channel.SendMessageAsync(":musical_note: I am already in the same audio channel as you!");
					return;
				}

				await channel.SendMessageAsync(
					":musical_note: Sorry, but I am already playing in a different audio channel at the moment.");

				return;
			}

			IAudioClient audio = await target.ConnectAsync(); //Connect to the voice channel

			ServerMusicItem item = new ServerMusicItem //Add it to the currentChannels list
			{
				GuildId = guild.Id,
				IsPlaying = false,
				IsExit = false,
				AudioClient = audio,
				AudioChannel = (SocketVoiceChannel) target,
				StartChannel = (ISocketMessageChannel) channel,
				AudioMusicFilesDownloader = null
			};

			currentChannels.Add(item);
		}

		/// <summary>
		/// Leaves the current voice channel the bot is in
		/// </summary>
		/// <param name="guild">The <see cref="IGuild"/> where the channel to leave is in</param>
		/// <param name="channel">The <see cref="IMessageChannel"/> to log messages to</param>
		/// <param name="user">The <see cref="IUser"/> who requested this command</param>
		/// <returns></returns>
		public async Task LeaveAudio(IGuild guild, IMessageChannel channel, IUser user)
		{
			if (guild == null) return; //Check if guild is null
			ServerMusicItem serverList = GetMusicList(guild.Id);
			if (serverList == null)
			{
				await channel.SendMessageAsync(":musical_note: You are not in any voice channel!");
				return;
			}

			//Check to see if the user is in the playing voice channel
			if (serverList.AudioChannel.GetUser(user.Id) == null)
			{
				await channel.SendMessageAsync(":musical_note: You are not in the current playing channel!");
				return;
			}

			serverList.IsExit = true;

			//Close ffmpeg if it is running
			if (serverList.FfMpeg != null)
			{
				serverList.FfMpeg.Kill();
				serverList.FfMpeg.Dispose();

				//Just wait a moment
				await Task.Delay(1000);
			}

			await serverList.AudioClient.StopAsync();

			serverList.IsPlaying = false;

			currentChannels.Remove(GetMusicList(guild.Id)); //Remove it from the currentChannels list
		}

		/// <summary>
		/// Stops playing the current song
		/// </summary>
		/// <param name="guild">The guild of <see cref="IMessageChannel"/></param>
		/// <param name="channel">The <see cref="IMessageChannel"/> to use for messages</param>
		/// <param name="user">The <see cref="IUser"/> who requested this command</param>
		/// <returns></returns>
		public async Task StopAudio(IGuild guild, IMessageChannel channel, IUser user)
		{
			ServerMusicItem serverList = GetMusicList(guild.Id);

			if (serverList == null)
			{
				await channel.SendMessageAsync(":musical_note: Your not in any voice channel!");
				return;
			}

			//Check to see if the user is in the playing audio channel
			if (serverList.AudioChannel.GetUser(user.Id) == null)
			{
				await channel.SendMessageAsync(":musical_note: You are not in the current playing channel!");
				return;
			}

			if (serverList.IsPlaying == false) await channel.SendMessageAsync(":musical_note: No audio is playing.");

			serverList.IsExit = true;
			await channel.SendMessageAsync(":musical_note: Stopping current playing song.");
		}

		/// <summary>
		/// Plays a song in a given voice channel
		/// </summary>
		/// <param name="guild">The <see cref="SocketGuild"/> where we are playing in</param>
		/// <param name="channel">The <see cref="IMessageChannel"/> to log messages to</param>
		/// <param name="target">The target <see cref="IVoiceChannel"/> to play music in</param>
		/// <param name="user">The <see cref="IUser"/> who requested this command</param>
		/// <param name="search">The query to search for</param>
		/// <returns></returns>
		public async Task SendAudio(SocketGuild guild, IMessageChannel channel, IVoiceChannel target, IUser user,
			string search)
		{
			ServerMusicItem serverMusicList = GetMusicList(guild.Id);

			//Join the voice channel the user is in if we are already not in a voice channel
			if (serverMusicList == null)
			{
				await JoinAudio(guild, target, channel, user);

				serverMusicList = GetMusicList(guild.Id);
			}

			//Check to see if the user is in the playing voice channel
			if (serverMusicList.AudioChannel.GetUser(user.Id) == null)
			{
				await channel.SendMessageAsync(":musical_note: You are not in the current playing channel!");
				return;
			}

			//Make sure the search isn't empty or null
			if (string.IsNullOrWhiteSpace(search))
			{
				await channel.SendMessageAsync("You need to input a search!");
				return;
			}

			IUserMessage message =
				await channel.SendMessageAsync($":musical_note: Preparing to search for '{search}'");

			string songFileLocation;
			string songName;

			search = AudioCheckService.RemovedNotAllowedChars(search);

			try
			{
				//We are downloading a direct URL
				if (WebUtils.IsStringValidUrl(search))
				{
					if (search.Contains("www.youtube.com/"))
					{
						await MessageUtils.ModifyMessage(message, ":musical_note: Processing YouTube URL...");

						Uri uri = new Uri(search);

						//Get video ID
						NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
						string videoId = query.AllKeys.Contains("v") ? query["v"] : uri.Segments.Last();

						if (videoId == "/")
						{
							await MessageUtils.ModifyMessage(message,
								":musical_note: The imputed URL is not a valid YouTube URL!");
							return;
						}

						await StopMusicFileDownloader();
						serverMusicList.AudioMusicFilesDownloader =
							new AudioDownloadMusicFiles(message, guild, Config.bot.AudioSettings.MaxVideoTime);
						songFileLocation = serverMusicList.AudioMusicFilesDownloader.DownloadAudioById(videoId);

						serverMusicList.AudioMusicFilesDownloader.Dispose();
					}
					else
					{
						await MessageUtils.ModifyMessage(message,
							":musical_note: The imputed URL is not a YouTube URL!");
						return;
					}
				}
				else //Search and download normally
				{
					await MessageUtils.ModifyMessage(message,
						$":musical_note: Searching my audio banks for '{search}'");

					songFileLocation = SearchMusicDirectory(search);

					//Search YouTube
					if (string.IsNullOrWhiteSpace(songFileLocation))
					{
						await StopMusicFileDownloader();
						serverMusicList.AudioMusicFilesDownloader =
							new AudioDownloadMusicFiles(message, guild, Config.bot.AudioSettings.MaxVideoTime);
						songFileLocation = serverMusicList.AudioMusicFilesDownloader.DownloadAudioByTitle(search);
						serverMusicList.AudioMusicFilesDownloader.Dispose();
					}
				}

				if (songFileLocation == null)
					return;

				Logger.Log($"Playing song from {songFileLocation}", LogVerbosity.Debug);

				string songFileName = Path.GetFileName(songFileLocation);
				songName = songFileName.Replace(".mp3",
					""); //This is so we say "Now playing 'Epic Song'" instead of "Now playing 'Epic Song.mp3'"

				//There is already a song playing, cancel it
				if (serverMusicList.IsPlaying)
				{
					serverMusicList.IsExit = true;

					while (serverMusicList.FfMpeg != null)
					{
						await Task.Delay(100);
					}

					await serverMusicList.Discord.FlushAsync();

					//Wait a moment
					await Task.Delay(100);
				}
			}
			catch (Exception ex)
			{
				Logger.Log(ex.ToString(), LogVerbosity.Error);
				return;
			}

			await Task.Delay(100);

			IAudioClient client = serverMusicList.AudioClient; //Make a reference to our AudioClient so it is easier
			Process ffmpeg = serverMusicList.FfMpeg = CreateFfmpeg(songFileLocation); //Start ffmpeg

			if (Config.bot.AudioSettings.LogPlayStopSongToConsole)
				Logger.Log($"The song '{songName}' on server {guild.Name}({guild.Id}) has started.",
					LogVerbosity.Music);

			await using Stream output = ffmpeg.StandardOutput.BaseStream; //ffmpeg base stream
			await using (serverMusicList.Discord = client.CreatePCMStream(AudioApplication.Music)
			) //Create an outgoing pcm stream
			{
				serverMusicList.IsPlaying = true;
				serverMusicList.IsExit = false;

				bool fail = false;
				bool exit = false;
				const int bufferSize = 1024;
				byte[] buffer = new byte[bufferSize];

				CancellationToken cancellation = new CancellationToken();

				await MessageUtils.ModifyMessage(message, $":musical_note: Now playing **{songName}**.");

				while (!fail && !exit)
				{
					try
					{
						if (cancellation.IsCancellationRequested)
						{
							exit = true;
							break;
						}

						if (serverMusicList.IsExit)
						{
							exit = true;
							break;
						}

						//Read our ffmpeg stream
						int read = await output.ReadAsync(buffer, 0, bufferSize, cancellation);
						if (read == 0)
						{
							exit = true;
							break;
						}

						//Write it to the audio out stream
						await serverMusicList.Discord.WriteAsync(buffer, 0, read, cancellation);

						if (serverMusicList.IsPlaying) continue;

						//For pausing the song
						do
						{
							//Do nothing, wait till is playing is true
							await Task.Delay(100, cancellation);
						} while (serverMusicList.IsPlaying == false);
					}
					catch (OperationCanceledException)
					{
					}
					catch (Exception ex)
					{
						await channel.SendMessageAsync("Sorry, but an error occured while playing!");

						if (Config.bot.ReportErrorsToOwner)
							await Global.BotOwner.SendMessageAsync(
								$"ERROR: {ex.Message}\nError occured while playing music on guild `{guild.Id}`.");

						fail = true;
					}
				}

				if (Config.bot.AudioSettings.LogPlayStopSongToConsole)
					Logger.Log($"The song '{songName}' on server {guild.Name}({guild.Id}) has stopped.",
						LogVerbosity.Music);

				//Clean up
				await serverMusicList.Discord.FlushAsync(cancellation);
				serverMusicList.Discord.Dispose();
				serverMusicList.IsPlaying = false;

				await channel.SendMessageAsync($":musical_note: **{songName}** ended or was stopped.");

				//Check to make sure that ffmpeg was disposed
				ffmpeg.Dispose();
				serverMusicList.FfMpeg = null;
			}

			async Task StopMusicFileDownloader()
			{
				if (serverMusicList.AudioMusicFilesDownloader != null)
				{
					serverMusicList.AudioMusicFilesDownloader.Dispose();
					await Task.Delay(100); //Wait a moment so the previous download can cancel and clean up
				}
			}
		}

		/// <summary>
		/// Pauses the current music playback
		/// </summary>
		/// <param name="guild">The <see cref="IGuild"/> that it is in</param>
		/// <param name="channel">The <see cref="IMessageChannel"/> to log messages to</param>
		/// <param name="user">The <see cref="IUser"/> who requested the command</param>
		/// <returns></returns>
		public async Task PauseAudio(IGuild guild, IMessageChannel channel, IUser user)
		{
			if (guild == null) return;

			ServerMusicItem musicList = GetMusicList(guild.Id);
			if (musicList == null) //The bot isn't in any voice channels
			{
				await channel.SendMessageAsync(":musical_note: There is no music being played!");
				return;
			}

			//Check to see if the user is in the playing audio channel
			if (musicList.AudioChannel.GetUser(user.Id) == null)
			{
				await channel.SendMessageAsync(":musical_note: You are not in the current playing channel!");
				return;
			}

			//Toggle pause status
			musicList.IsPlaying = !musicList.IsPlaying;

			if (musicList.IsPlaying) await channel.SendMessageAsync(":musical_note: Current song has been un-paused.");
			else await channel.SendMessageAsync(":musical_note: Current song has been paused.");
		}

		/// <summary>
		/// Searches music folder for similar or same results to <see cref="search"/>
		/// </summary>
		/// <param name="search">The name of the song to search for</param>
		/// <returns>Returns the first found similar or matching result</returns>
		public static string SearchMusicDirectory(string search)
		{
			if (!Directory.Exists(MusicDir)) Directory.CreateDirectory(MusicDir);

			DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(MusicDir);
			FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + search + "*.mp3");

			return filesInDir.Select(foundFile => foundFile.FullName).FirstOrDefault();
		}

		/// <summary>
		/// Creates and returns a ffmpeg <see cref="Process"/>
		/// </summary>
		/// <param name="path">The path of the song to play</param>
		/// <returns>Returns the newly created ffmpeg <see cref="Process"/></returns>
		private static Process CreateFfmpeg(string path)
		{
			return Process.Start(new ProcessStartInfo
			{
				FileName = Config.bot.AudioSettings.FfmpegLocation,
				Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardInput = false
			});
		}

		#region List Fuctions

		private static ServerMusicItem GetMusicList(ulong guildId)
		{
			IEnumerable<ServerMusicItem> result = from a in currentChannels
				where a.GuildId == guildId
				select a;

			ServerMusicItem list = result.FirstOrDefault();
			return list;
		}

		#endregion
	}
}