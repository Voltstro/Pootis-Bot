using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Pootis_Bot.Core;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Services.Audio
{
	public static class AudioCheckService
	{
		//Downloads a .json file that has were to get some needed libs from
		private const string AudioLibFileJsonUrl = "https://pootis-bot.creepysin.com/download/audiolibfiles.json";

		//This is for when we are downloading audio service files
		private static List<AudioDownloadServiceFiles.LibFile> _libFiles;

		/// <summary>
		///     Checks the audio service
		/// </summary>
		public static void CheckAudioService()
		{
			if (!Config.bot.AudioSettings.AudioServicesEnabled) return;
			Global.Log("Checking audio services...", ConsoleColor.Blue);

			if (!Environment.Is64BitProcess)
			{
				Global.Log("Audio services cannot run on a 32-bit machine/process! Audio services weren't enabled.",
					ConsoleColor.Blue);

				Config.bot.AudioSettings.AudioServicesEnabled = false;
				Config.SaveConfig();

				return;
			}

			//Check to see if all the necessary files are here.
			if (!File.Exists("external/ffmpeg.exe") || !File.Exists("external/ffplay.exe") ||
			    !File.Exists("external/ffprobe.exe")
			    || !File.Exists("opus.dll") || !File.Exists("libsodium.dll")) UpdateAudioFiles();

			if (string.IsNullOrWhiteSpace(Config.bot.Apis.ApiYoutubeKey))
			{
				Global.Log(
					"You need to set a YouTube Data API key! You can get one from https://console.developers.google.com and creating a new project with the YouTube Data API v3, and setting via the config menu.",
					ConsoleColor.Red);

				Global.Log("Audio service was disabled!", ConsoleColor.Red);

				Config.bot.AudioSettings.AudioServicesEnabled = false;
				Config.SaveConfig();
			}
			else
			{
				if (Config.bot.AudioSettings.AudioServicesEnabled)
					Global.Log("Audio services are ready!", ConsoleColor.Blue);
			}
		}

		/// <summary>
		///     Removes not allowed characters that can't be in a windows file name
		/// </summary>
		/// <param name="input"></param>
		/// <returns>Formatted string</returns>
		public static string RemovedNotAllowedChars(string input)
		{
			//Remove quotes and other symbols
			string unQuoted = input.Replace("&quot;", "'").Replace(":", "").Replace("|", "-");

			string decoded = WebUtility.HtmlDecode(unQuoted);
			//Remove html formatting tags
			return Regex.Replace(decoded, "<.*?>", string.Empty);
		}

		/// <summary>
		///     Updates all files related to audio
		/// </summary>
		public static void UpdateAudioFiles()
		{
			Global.Log("Downloading required files for audio services...");

			//If the temp directory doesn't exist, create a new one.
			if (!Directory.Exists("Temp/")) Directory.CreateDirectory("temp/");

			//If the external directory doesn't exist, create it
			if (!Directory.Exists("External/")) Directory.CreateDirectory("External/");


			// ReSharper disable once CommentTypo
			//Get the audiolibfiles.json from the pootis-bot website and deserialize it.
			Global.Log($"Gathering needed information from {AudioLibFileJsonUrl}...");

			string json = WebUtils.DownloadString(AudioLibFileJsonUrl);

			_libFiles = JsonConvert.DeserializeObject<List<AudioDownloadServiceFiles.LibFile>>(json);
			Global.Log("Got what I needed.");

			//Download required files depending on platform
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				AudioDownloadServiceFiles.PrepareWindowFiles(GetLibFile("Windows"));
			}
			else
			{
				Global.Log("Currently platform not supported! Audio services have been disabled!");
				Config.bot.AudioSettings.AudioServicesEnabled = false;
				Config.SaveConfig();

				return;
			}

			Global.Log("Done! All files needed for audio service are ready!", ConsoleColor.Blue);
		}

		private static AudioDownloadServiceFiles.LibFile GetLibFile(string osPlatform)
		{
			IEnumerable<AudioDownloadServiceFiles.LibFile> result = from a in _libFiles
				where a.OsPlatform == osPlatform
				select a;

			AudioDownloadServiceFiles.LibFile libFile = result.FirstOrDefault();
			return libFile;
		}
	}
}