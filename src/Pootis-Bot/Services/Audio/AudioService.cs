using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Services.Audio
{
	public class AudioService
	{
		private const string FfmpegLocation = "External/ffmpeg";
		private const string MusicDir = "Music/";

		public static readonly List<ServerMusicItem> currentChannels = new List<ServerMusicItem>();

		/// <summary>
		///     Joins a guild voice channel, and sends messages to a text channel on error
		/// </summary>
		/// <param name="guild">What guild are we in</param>
		/// <param name="target">The voice channel we are attempting to join</param>
		/// <param name="channel">The message channel to log errors in</param>
		/// <returns></returns>
		public async Task JoinAudio(IGuild guild, IVoiceChannel target, IMessageChannel channel)
		{
			if (target == null)
			{
				await channel.SendMessageAsync(":musical_note: You need to be in a voice channel!");
				return;
			}

			ServerMusicItem serverMusic = GetMusicList(guild.Id);
			if (serverMusic != null)
			{
				await channel.SendMessageAsync(
					":musical_note: Sorry, but I am already playing in a different audio channel at the moment.");

				return;
			}

			IAudioClient audio = await target.ConnectAsync(); //Connect to the voice channel

			ServerMusicItem item = new ServerMusicItem //Added it to the currentChannels list
			{
				GuildId = guild.Id,
				IsPlaying = false,
				IsExit = false,
				AudioClient = audio,
				AudioChannel = (SocketVoiceChannel) target,
				StartChannel = (ISocketMessageChannel) channel
			};

			currentChannels.Add(item);
		}

		/// <summary>
		///     Leaves an audio channel
		/// </summary>
		/// <param name="guild">The current guild</param>
		/// <param name="channel">Where the messages are sent</param>
		/// <param name="user"></param>
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

			//Check to see if the user is in the playing audio channel
			if (serverList.AudioChannel.GetUser(user.Id) == null)
			{
				await channel.SendMessageAsync(":musical_note: You are not in the current playing channel!");
				return;
			}

			serverList.IsExit = true;

			if (serverList.FfMpeg != null)
			{
				serverList.FfMpeg.Kill();
				serverList.FfMpeg.Dispose();
			}

			//Just wait a moment
			await Task.Delay(1000);

			await serverList.AudioClient.StopAsync();

			serverList.IsPlaying = false;

			currentChannels.Remove(GetMusicList(guild.Id)); //Remove it from the currentChannels list
		}

		/// <summary>
		///     Leaves the audio channel
		/// </summary>
		/// <param name="guild">The guild of <see cref="channel" /></param>
		/// <param name="channel">The channel to use for messages</param>
		/// <param name="user"></param>
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
		///     Plays a song in a given voice channel
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <param name="target"></param>
		/// <param name="user"></param>
		/// <param name="search">The name of the song to play</param>
		/// <returns></returns>
		public async Task SendAudio(SocketGuild guild, IMessageChannel channel, IVoiceChannel target, IUser user,
			string search)
		{
			ServerMusicItem serverList = GetMusicList(guild.Id);

			if (serverList == null)
			{
				await JoinAudio(guild, target, channel);

				serverList = GetMusicList(guild.Id);
			}

			//Check to see if the user is in the playing audio channel
			if (serverList.AudioChannel.GetUser(user.Id) == null)
			{
				await channel.SendMessageAsync(":musical_note: You are not in the current playing channel!");
				return;
			}

			if (string.IsNullOrWhiteSpace(search))
			{
				await channel.SendMessageAsync("You need to input a search!");
				return;
			}

			IUserMessage message =
				await channel.SendMessageAsync($":musical_note: Searching my audio banks for '{search}'");

			string fileLoc;
			string fileName;

			try
			{
				fileLoc = SearchAudio(search); //Search for the song in our current music directory

				//The search didn't come up with anything, lets attempt to get it from YouTube
				if (string.IsNullOrWhiteSpace(fileLoc))
				{
					AudioDownloadMusicFiles audioDownload = new AudioDownloadMusicFiles();
					string result = audioDownload.DownloadAudio(search, message, guild);
					audioDownload.Dispose();

					if (result != null)
						fileLoc = result;
					else
						return;
				}
				
				Debug.WriteLine(fileLoc);

				string tempName = Path.GetFileName(fileLoc);
				fileName = tempName.Replace(".mp3", "");

				if (serverList.IsPlaying)
				{
					//Kill and dispose of ffmpeg
					serverList.FfMpeg.Kill();
					serverList.FfMpeg.Dispose();

					await serverList.Discord.FlushAsync();

					//Wait a moment
					await Task.Delay(100);
				}
			}
			catch (Exception ex)
			{
				Global.Log(ex.Message, ConsoleColor.Red);
				return;
			}

			IAudioClient client = serverList.AudioClient;
			Process ffmpeg = serverList.FfMpeg = GetFfmpeg(fileLoc);

			if (Config.bot.AudioSettings.LogPlayStopSongToConsole)
				Global.Log($"The song '{fileName}' on server {guild.Name}({guild.Id}) has started.", ConsoleColor.Blue);

			await using Stream output = ffmpeg.StandardOutput.BaseStream;
			await using (serverList.Discord = client.CreatePCMStream(AudioApplication.Music))
			{
				serverList.IsPlaying = true;
				bool fail = false;
				bool exit = false;
				int bufferSize = 1024;
				byte[] buffer = new byte[bufferSize];

				CancellationToken cancellation = new CancellationToken();

				await message.ModifyAsync(x => { x.Content = $":musical_note: Now playing **{fileName}**."; });

				//await channel.SendMessageAsync($":musical_note: Now playing **{fileName}**.");

				serverList.IsExit = false;

				//To be completely honest, I don't understand much of this.
				//Pootis-Bot and the audio services have always been difficult for me.
				//If anyone could improve upon this I would gladly accept it!
				while (!fail && !exit)
					try
					{
						if (cancellation.IsCancellationRequested)
						{
							exit = true;
							break;
						}

						if (serverList.IsExit)
						{
							exit = true;
							break;
						}

						int read = await output.ReadAsync(buffer, 0, bufferSize, cancellation);
						if (read == 0)
						{
							exit = true;
							break;
						}

						await serverList.Discord.WriteAsync(buffer, 0, read, cancellation);

						if (serverList.IsPlaying) continue;

						do
						{
							//Do nothing, wait till is playing is true
							await Task.Delay(100, cancellation);
						} while (serverList.IsPlaying == false);
					}
					catch (OperationCanceledException)
					{
					}
					catch (Exception ex)
					{
						await channel.SendMessageAsync($"Sorry an error occured **Error Details**\n{ex.Message}");

						if (Config.bot.ReportErrorsToOwner)
							await Global.BotOwner.SendMessageAsync(
								$"ERROR: {ex.Message}\nError occured while playing music on guild `{guild.Id}`.");

						fail = true;
					}

				//End
				if (Config.bot.AudioSettings.LogPlayStopSongToConsole)
					Global.Log($"The song '{fileName}' on server {guild.Name}({guild.Id}) has stopped.",
						ConsoleColor.Blue);

				await serverList.Discord.FlushAsync(cancellation);
				serverList.Discord.Dispose();
				serverList.IsPlaying = false;

				await channel.SendMessageAsync($":musical_note: **{fileName}** ended or was stopped.");

				//Check to make sure that ffmpeg was disposed
				ffmpeg.Dispose();

				serverList.FfMpeg = null;
			}
		}

		/// <summary>
		///     Pauses audio playback for a voice channel
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task PauseAudio(IGuild guild, IMessageChannel channel, IUser user)
		{
			if (guild == null) return;

			ServerMusicItem musicList = GetMusicList(guild.Id);
			if (musicList == null)
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

			musicList.IsPlaying = !musicList.IsPlaying; //Toggle pause status

			if (musicList.IsPlaying) await channel.SendMessageAsync(":musical_note: Current song has been un-paused.");
			else await channel.SendMessageAsync(":musical_note: Current song has been paused.");
		}

		/// <summary>
		///     Searches the music directory for a downloaded audio file
		/// </summary>
		/// <param name="search"></param>
		/// <returns></returns>
		public static string SearchAudio(string search)
		{
			if (!Directory.Exists(MusicDir)) Directory.CreateDirectory(MusicDir);

			DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(MusicDir);
			FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + search + "*.mp3");

			return filesInDir.Select(foundFile => foundFile.FullName).FirstOrDefault();
		}

		/// <summary>
		///     Gets a process running ffmpeg
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static Process GetFfmpeg(string path)
		{
			return Process.Start(new ProcessStartInfo
			{
				FileName = FfmpegLocation,
				Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardInput = false
			});
		}

		#region List Fuctions

		private ServerMusicItem GetMusicList(ulong guildid)
		{
			IEnumerable<ServerMusicItem> result = from a in currentChannels
				where a.GuildId == guildid
				select a;

			ServerMusicItem list = result.FirstOrDefault();
			return list;
		}

		#endregion
	}
}