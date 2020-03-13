using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
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
			Logger.Log("Checking audio services...", LogVerbosity.Music);

			if (!Environment.Is64BitProcess)
			{
				Logger.Log("Audio services cannot run on a 32-bit machine/process! Audio services weren't enabled.",
					LogVerbosity.Music);

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
			Logger.Log($"Audio services for unix systems are currently disabled! Please check out {Global.githubPage}/issues/2");
			Config.bot.AudioSettings.AudioServicesEnabled = false;
			Config.SaveConfig();

			return;

			//Check files to see if they exist
			//if(!File.Exists("External/ffmpeg") || !File.Exists("External/ffprobe") || !File.Exists("opus.dll") || !File.Exists("libsodium.dll"))
			//  	UpdateAudioFiles();

#elif OSX
			Logger.Log($"Audio services for unix systems are currently disabled! Please check out {Global.githubPage}/issues/2");
			Config.bot.AudioSettings.AudioServicesEnabled = false;
			Config.SaveConfig();

			return;

			//Yep, you guessed it, check the files
			//if(!File.Exists("External/ffmpeg") || !File.Exists("External/ffprobe") || !File.Exists("External/ffplay"))
			//  	UpdateAudioFiles();
#endif

			if (string.IsNullOrWhiteSpace(Config.bot.Apis.ApiYoutubeKey))
			{
				Logger.Log(
					"You need to set a YouTube Data API key! You can get one from https://console.developers.google.com and creating a new project with the YouTube Data API v3, and setting via the config menu.",
					LogVerbosity.Music);

				Logger.Log("Audio service was disabled!", LogVerbosity.Music);

				Config.bot.AudioSettings.AudioServicesEnabled = false;
				Config.SaveConfig();
			}
			else
			{
				if (Config.bot.AudioSettings.AudioServicesEnabled)
					Logger.Log("Audio services are ready!", LogVerbosity.Music);
			}
		}

		/// <summary>
		/// Removes not allowed characters that can't be in a windows file name
		/// </summary>
		/// <param name="input"></param>
		/// <returns>Formatted string</returns>
		public static string RemovedNotAllowedChars(string input)
		{
			//TODO: Re-write this when I move all string stuff to one class

			//Remove quotes and other symbols
			string unQuoted = input.Replace("&quot;", "'").Replace(":", "").Replace("|", "-").Replace("\"", "'");

			string decoded = WebUtility.HtmlDecode(unQuoted);
			//Remove html formatting tags
			return Regex.Replace(decoded, "<.*?>", string.Empty);
		}

		/// <summary>
		/// Updates all files related to audio
		/// </summary>
		public static void UpdateAudioFiles()
		{
			Logger.Log("Downloading required files for audio services...");

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

			Logger.Log("Done! All files needed for audio service are ready!", LogVerbosity.Music);
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