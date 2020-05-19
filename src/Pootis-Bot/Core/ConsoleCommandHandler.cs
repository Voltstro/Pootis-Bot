using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.ConsoleCommandHandler;
using Pootis_Bot.Core.ConfigMenuPlus;
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
				$"Unknown command! Type `help` for help.";
			UnknownCommandErrorColor = ConsoleColor.Red;

			//Add all of our commands
			AddCommand("help", "Lists all commands", HelpCmd);
			AddCommand("exit", "Shuts down the bot", ExitCmd);
			AddCommand("config", "Opens the config menu", OpenConfigCmd);
			AddCommand("version", "Returns the current version of the bot you are using", VersionCmd);
			AddCommand("about", "Returns a simple about screen", AboutCmd);
			AddCommand("setgame", "Enters into the setgame menu", SetGameStatusCmd);
			AddCommand("togglestream", "Toggles the bot between streaming mode and not", SetStreamingStatusCmd);
			AddCommand("deletemusic", "Deletes all saved music", DeleteMusicCmd);
			AddCommand("toggleaudio", "Toggles having the audio services enabled and disabled", ToggleAudioCmd);
			AddCommand("forceaudioupdate", "Forces the audio services files to update", ForceAudioUpdateCmd);
			AddCommand("status", "Shows the bot's current status", StatusCmd);
			AddCommand("clear", "Clears the screen", ClearCmd);
			AddCommand("resethelpmodules", "Resets the help modules to the default", ResetHelpModulesCmd);
			AddCommand("save config", "Saves the config file", SaveConfigCmd);
			AddCommand("save accounts", "Saves the accounts file", SaveAccountsCmd);
			AddCommand("save servers", "Saves the server list file", SaveServersCmd);
			AddCommand("info", "Displays system and bot info", Info);

			ConsoleHandleLoop();
		}

		public override void LogMessage(string message, ConsoleColor color)
		{
			Logger.Log(message, LogVerbosity.Error);
		}

		private void HelpCmd()
		{
			Dictionary<string, ConsoleCommand> commands = GetAllInstalledConsoleCommands();
			StringBuilder commandsWithSummary = new StringBuilder();
			commandsWithSummary.Append("==== Command List ====\n");

			foreach ((string _, ConsoleCommand command) in commands)
			{
				commandsWithSummary.Append($"`{command.CommandName}` - {command.CommandSummary}\n");
			}

			commandsWithSummary.Append($"For more info visit {Global.websiteConsoleCommands}");

			System.Console.WriteLine(commandsWithSummary);
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
				//If there is already a song playing, cancel it
				await AudioService.StopPlayingAudioOnServer(channel);

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
			new ConfigMainMenu().OpenConfigMenu();
			//new ConfigMenu().OpenConfig();
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
				await AudioService.StopPlayingAudioOnServer(channel);

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
				MusicLibsChecker.CheckAudioService();
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

			MusicLibsChecker.UpdateAudioFiles();
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

			Logger.Log("The help modules were reset to their defaults.");
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