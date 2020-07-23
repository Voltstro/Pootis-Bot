using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Pootis_Bot.ConsoleCommandHandler;
using Pootis_Bot.Core.ConfigMenuPlus;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services.Audio.Music;
using Pootis_Bot.Services.Audio.Music.ExternalLibsManagement;
using Console = Pootis_Bot.ConsoleCommandHandler.Console;

namespace Pootis_Bot.Core
{
	/// <summary>
	/// Handles Pootis-Bot's console
	/// </summary>
	public class ConsoleCommandHandler : Console
	{
		private readonly Bot bot;

		public ConsoleCommandHandler(Bot bot)
		{
			this.bot = bot;
		}

		/// <summary>
		/// Sets up the the Pootis-Bot's <see cref="Console"/> to handle commands and such
		/// </summary>
		public void SetupConsole()
		{
			UnknownCommandError =
				"Unknown command! Type `help` for help.";
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
			Logger.Error(message);
		}

		private void HelpCmd()
		{
			Dictionary<string, ConsoleCommand> commands = GetAllInstalledConsoleCommands();
			StringBuilder commandsWithSummary = new StringBuilder();
			commandsWithSummary.Append("==== Command List ====\n");

			foreach ((string _, ConsoleCommand command) in commands)
				commandsWithSummary.Append($"`{command.CommandName}` - {command.CommandSummary}\n");

			commandsWithSummary.Append($"For more info visit {Global.websiteConsoleCommands}");

			System.Console.WriteLine(commandsWithSummary);
		}

		private async void ExitCmd()
		{
			IsExiting = true;

			Logger.Info("Shutting down...");

			await bot.EndBot();

			Environment.Exit(0);
		}

		private static void OpenConfigCmd()
		{
			new ConfigMainMenu().OpenConfigMenu();
		}

		private static void VersionCmd()
		{
			Logger.Info($"You are running version {VersionUtils.GetAppVersion()} of Pootis-Bot!");
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

			await bot.Client.SetGameAsync(Global.BotStatusText, twitch, activity);

			Logger.Info($"Bot's game status was set to '{Global.BotStatusText}'");
		}

		private async void SetStreamingStatusCmd()
		{
			if (Bot.IsStreaming)
			{
				Bot.IsStreaming = false;
				await bot.Client.SetGameAsync(Global.BotStatusText, "");
				Logger.Info("Bot no longer shows streaming status.");
			}
			else
			{
				Bot.IsStreaming = true;
				await bot.Client.SetGameAsync(Global.BotStatusText, Config.bot.TwitchStreamingSite,
					ActivityType.Streaming);

				Logger.Info("Bot now shows streaming status.");
			}
		}

		private static async void DeleteMusicCmd()
		{
			foreach (ServerMusicItem channel in MusicService.currentChannels)
			{
				await MusicService.StopPlayingAudioOnServer(channel);

				//Just wait a moment
				await Task.Delay(100);

				await channel.AudioClient.StopAsync();

				channel.IsPlaying = false;
			}

			MusicService.currentChannels.Clear();

			Logger.Info("Deleting music directory...");
			if (Directory.Exists("Music/"))
			{
				Directory.Delete("Music/", true);
				Logger.Info("Done!");
			}
			else
			{
				Logger.Error("The music directory doesn't exist!");
			}
		}

		private static void ToggleAudioCmd()
		{
			Config.bot.AudioSettings.AudioServicesEnabled = !Config.bot.AudioSettings.AudioServicesEnabled;
			Config.SaveConfig();

			Logger.Info("The audio service was set to {@AudioServicesEnabled}", Config.bot.AudioSettings.AudioServicesEnabled);
			if (Config.bot.AudioSettings.AudioServicesEnabled)
				MusicLibsChecker.CheckMusicService();
		}

		private static async void ForceAudioUpdateCmd()
		{
			Logger.Info("Updating audio files.");
			foreach (ServerMusicItem channel in MusicService.currentChannels)
				channel.AudioClient.Dispose();

			await Task.Delay(1000);

			//Delete old files first
			Directory.Delete(Config.bot.AudioSettings.ExternalDirectory, true);

			MusicLibsChecker.GetLibsPreparer().DeleteFiles();

			MusicLibsChecker.CheckMusicService(true);
			Logger.Info("Audio files were updated.");
		}

		private void StatusCmd()
		{
			Logger.Info("Bot status: {@ConnectionState}\nServer count: {@GuildsCount}\nLatency: {@ClientLatency}", bot.Client.ConnectionState, bot.Client.Guilds.Count, bot.Client.Latency);
		}

		private static void ClearCmd()
		{
			System.Console.Clear();
		}

		private static void ResetHelpModulesCmd()
		{
			HelpModulesManager.ResetHelpModulesToDefault();
			HelpModulesManager.SaveHelpModules();

			Logger.Info("The help modules were reset to their defaults.");
		}

		private static void SaveConfigCmd()
		{
			Config.SaveConfig();
			Logger.Info("Config saved!");
		}

		private static void SaveAccountsCmd()
		{
			UserAccountsManager.SaveAccounts();
			Logger.Info("User accounts saved!");
		}

		private static void SaveServersCmd()
		{
			ServerListsManager.SaveServerList();
			Logger.Info("Server list saved!");
		}

		private static void Info()
		{
			Logger.Info("==== System Info ====");
			Logger.Info(" - OS Version:          {@OSVersion}", Environment.OSVersion.Version.ToString());
			Logger.Info(" - OS Name:             {@OSDescription}", RuntimeInformation.OSDescription);
			Logger.Info(" - System Architecture: {@OSArchitecture}", RuntimeInformation.OSArchitecture);
			Logger.Info(" - NET Core:            {@FrameworkDescription}", RuntimeInformation.FrameworkDescription);
			Logger.Info("");
			Logger.Info("=== Pootis-Bot Info ====");
			Logger.Info(" - Version:             {@AppVersion}", VersionUtils.GetAppVersion());
			Logger.Info(" - Discord.Net Version: {@DiscordNetVersion}", VersionUtils.GetDiscordNetVersion());
		}
	}
}