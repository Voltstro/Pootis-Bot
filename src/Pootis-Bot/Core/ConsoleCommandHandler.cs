using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services.Audio;
using Console = Pootis_Bot.ConsoleCommandHandler.Console;

namespace Pootis_Bot.Core
{
	public class ConsoleCommandHandler : Console
	{
		private readonly DiscordSocketClient _client;

		public ConsoleCommandHandler(DiscordSocketClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Sets up the the Pootis-Bot's <see cref="Console"/> to handle commands and such
		/// </summary>
		public void SetupConsole()
		{
			UnknownCommandError =
				$"Unknown command! Visit {Global.websiteConsoleCommands} for a list of console commands.";
			UnknownCommandErrorColor = ConsoleColor.Red;

			//Add all of our commands
			AddCommand("exit", ExitCmd);
			AddCommand("config", OpenConfigCmd);
			AddCommand("version", VersionCmd);
			AddCommand("about", AboutCmd);
			AddCommand("setgame", SetGameStatusCmd);
			AddCommand("togglestream", SetStreamingStatusCmd);
			AddCommand("deletemusic", DeleteMusicCmd);
			AddCommand("toggleaudio", ToggleAudioCmd);
			AddCommand("forceaudioupdate", ForceAudioUpdateCmd);
			AddCommand("status", StatusCmd);
			AddCommand("clear", ClearCmd);
			AddCommand("resethelpmodules", ResetHelpModulesCmd);
			AddCommand("save config", SaveConfigCmd);
			AddCommand("save accounts", SaveAccountsCmd);
			AddCommand("save servers", SaveServersCmd);
			AddCommand("info", Info);

			ConsoleHandleLoop();
		}

		public override void LogMessage(string message, ConsoleColor color)
		{
			Logger.Log(message);
		}

		private async void ExitCmd()
		{
			IsExiting = true;

			Bot.IsRunning = false;

			Logger.Log("Shutting down...");
			await _client.SetGameAsync("Bot shutting down");

			Logger.Log("Stopping audio services...", LogVerbosity.Music);
			foreach (ServerMusicItem channel in AudioService.currentChannels)
			{
				channel.IsExit = true;

				if (channel.FfMpeg != null)
				{
					channel.FfMpeg.Kill();
					channel.FfMpeg.Dispose();
				}

				//Just wait a moment
				await Task.Delay(100);

				await channel.AudioClient.StopAsync();

				Logger.Log($"Ended {channel.GuildId} audio session.", LogVerbosity.Debug);
			}

			await _client.LogoutAsync();
			_client.Dispose();

			//Clean up
			Global.HttpClient.Dispose();
			Logger.EndLogger();

			Environment.Exit(0);
		}

		private static void OpenConfigCmd()
		{
			new ConfigMenu().OpenConfig();
		}

		private static void VersionCmd()
		{
			Logger.Log($"You are running version {VersionUtils.GetAppVersion()} of Pootis-Bot!");
		}

		private static void AboutCmd()
		{
			System.Console.WriteLine(Global.aboutMessage);
		}

		private async void SetGameStatusCmd()
		{
			System.Console.WriteLine("Enter in what you want to set the bot's game to:");
			Global.BotStatusText = System.Console.ReadLine();

			ActivityType activity = ActivityType.Playing;
			string twitch = null;
			if (Bot.IsStreaming)
			{
				activity = ActivityType.Streaming;
				twitch = Config.bot.TwitchStreamingSite;
			}

			await _client.SetGameAsync(Global.BotStatusText, twitch, activity);

			Logger.Log($"Bot's game status was set to '{Global.BotStatusText}'");
		}

		private async void SetStreamingStatusCmd()
		{
			if (Bot.IsStreaming)
			{
				Bot.IsStreaming = false;
				await _client.SetGameAsync(Global.BotStatusText, "");
				Logger.Log("Bot no longer shows streaming status.");
			}
			else
			{
				Bot.IsStreaming = true;
				await _client.SetGameAsync(Global.BotStatusText, Config.bot.TwitchStreamingSite,
					ActivityType.Streaming);

				Logger.Log("Bot now shows streaming status.");
			}
		}

		private static async void DeleteMusicCmd()
		{
			foreach (ServerMusicItem channel in AudioService.currentChannels)
			{
				channel.IsExit = true;

				if (channel.FfMpeg != null)
				{
					channel.FfMpeg.Kill();
					channel.FfMpeg.Dispose();
				}

				//Just wait a moment
				await Task.Delay(100);

				await channel.AudioClient.StopAsync();

				channel.IsPlaying = false;
			}

			AudioService.currentChannels.Clear();

			Logger.Log("Deleting music directory...", LogVerbosity.Music);
			if (Directory.Exists("Music/"))
			{
				Directory.Delete("Music/", true);
				Logger.Log("Done!", LogVerbosity.Music);
			}
			else
			{
				Logger.Log("The music directory doesn't exist!", LogVerbosity.Music);
			}
		}

		private static void ToggleAudioCmd()
		{
			Config.bot.AudioSettings.AudioServicesEnabled = !Config.bot.AudioSettings.AudioServicesEnabled;
			Config.SaveConfig();

			Logger.Log($"The audio service was set to {Config.bot.AudioSettings.AudioServicesEnabled}",
				LogVerbosity.Music);
			if (Config.bot.AudioSettings.AudioServicesEnabled)
				AudioCheckService.CheckAudioService();
		}

		private static async void ForceAudioUpdateCmd()
		{
			Logger.Log("Updating audio files.", LogVerbosity.Music);
			foreach (ServerMusicItem channel in AudioService.currentChannels)
				channel.AudioClient.Dispose();

			await Task.Delay(1000);

			//Delete old files first
			Directory.Delete("External/", true);
			File.Delete("libsodium.dll");
			File.Delete("opus.dll");

			AudioCheckService.UpdateAudioFiles();
			Logger.Log("Audio files were updated.", LogVerbosity.Music);
		}

		private void StatusCmd()
		{
			Logger.Log(
				$"Bot status: {_client.ConnectionState.ToString()}\nServer count: {_client.Guilds.Count}\nLatency: {_client.Latency}");
		}

		private static void ClearCmd()
		{
			System.Console.Clear();
		}

		private static void ResetHelpModulesCmd()
		{
			HelpModulesManager.ResetHelpModulesToDefault();
			HelpModulesManager.SaveHelpModules();

			Logger.Log("The help modules were reset to there defaults.");
		}

		private static void SaveConfigCmd()
		{
			Config.SaveConfig();
			Logger.Log("Config saved!");
		}

		private static void SaveAccountsCmd()
		{
			UserAccountsManager.SaveAccounts();
			Logger.Log("User accounts saved!");
		}

		private static void SaveServersCmd()
		{
			ServerListsManager.SaveServerList();
			Logger.Log("Server list saved!");
		}

		private static void Info()
		{
			Logger.Log("==== System Info ====");
			Logger.Log($" - OS Version:          {Environment.OSVersion}");
			Logger.Log($" - OS Name:             {RuntimeInformation.OSDescription}");
			Logger.Log($" - System Architecture: {RuntimeInformation.OSArchitecture}");
			Logger.Log($" - NET Core:            {RuntimeInformation.FrameworkDescription}");
			Logger.Log("");
			Logger.Log("=== Pootis-Bot Info ====");
			Logger.Log($" - Version:             {VersionUtils.GetAppVersion()}");
			Logger.Log($" - Discord.Net Version: {VersionUtils.GetDiscordNetVersion()}");
		}
	}
}