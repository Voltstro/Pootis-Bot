using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Pootis_Bot.Core;
using Pootis_Bot.Helpers;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.Audio
{
	public static class AudioCheckService
	{
		//Downloads a .json file that has were to get some needed libs from
		private const string AudioLibFileJsonUrl = "https://pootis-bot.creepysin.com/download/externallibfiles.json";

		/// <summary>
		/// Checks the audio service
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

#if WINDOWS
			//Check to see if all the necessary files are here.
			if (!File.Exists("external/ffmpeg.exe") || !File.Exists("external/ffplay.exe") ||
			    !File.Exists("external/ffprobe.exe")
			    || !File.Exists("opus.dll") || !File.Exists("libsodium.dll")) UpdateAudioFiles();

#elif LINUX
			//Check files to see if they exist
			if(!File.Exists("External/ffmpeg") || !File.Exists("External/ffprobe") || !File.Exists("opus.dll") || !File.Exists("libsodium.dll"))
				UpdateAudioFiles();

#elif OSX
			//Yep, you guessed it, check the files
			if(!File.Exists("External/ffmpeg") || !File.Exists("External/ffprobe") || !File.Exists("External/ffplay"))
				UpdateAudioFiles();
#endif

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
		/// Removes not allowed characters that can't be in a windows file name
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
		/// Updates all files related to audio
		/// </summary>
		public static void UpdateAudioFiles()
		{
			Global.Log("Downloading required files for audio services...");

			//If the temp directory doesn't exist, create a new one.
			if (!Directory.Exists("Temp/")) Directory.CreateDirectory("Temp/");

			//If the external directory doesn't exist, create it
			if (!Directory.Exists("External/")) Directory.CreateDirectory("External/");

			List<AudioExternalLibFiles> listOfLibsFilesForOs =
				JsonConvert.DeserializeObject<List<AudioExternalLibFiles>>(
					WebUtils.DownloadString(AudioLibFileJsonUrl));

#if WINDOWS
			AudioDownloadServiceFiles.DownloadAndPrepareWindowsFiles(GetDownloadUrls(listOfLibsFilesForOs, "Windows"));
			Config.bot.AudioSettings.FfmpegLocation = "External/ffmpeg.exe";
#elif LINUX
			AudioDownloadServiceFiles.DownloadAndPrepareLinuxFiles(GetDownloadUrls(listOfLibsFilesForOs, "Linux"));
			Config.bot.AudioSettings.FfmpegLocation = "External/ffmpeg";
#elif OSX
			//TODO: Implement MacOs Downloading
#endif
			Config.SaveConfig();

			Global.Log("Done! All files needed for audio service are ready!", ConsoleColor.Blue);
		}

		private static AudioExternalLibFiles GetDownloadUrls(IEnumerable<AudioExternalLibFiles> audioExternalLibFiles,
			string osPlatform)
		{
			IEnumerable<AudioExternalLibFiles> result = from a in audioExternalLibFiles
				where a.OsPlatform == osPlatform
				select a;

			return result.FirstOrDefault();
		}
	}
}