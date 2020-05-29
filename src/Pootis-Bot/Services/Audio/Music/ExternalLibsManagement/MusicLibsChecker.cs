using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Helpers;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.Audio.Music.ExternalLibsManagement
{
	/// <summary>
	/// Checks to make sure require software and libs are installed, if not it downloads them
	/// </summary>
	public static class MusicLibsChecker
	{
		//Downloads a .json file that has were to get some needed libs from
		private const string AudioLibFileJsonUrl = "https://pootis-bot.voltstro.dev/download/externallibfiles.json";

		/// <summary>
		/// Checks the audio service
		/// </summary>
		public static void CheckMusicService(bool forceRedownload = false)
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

			//If YouTube services has disabled, we cannot use audio services
			if (!Config.bot.Apis.YouTubeService)
			{
				Config.bot.AudioSettings.AudioServicesEnabled = false;
				Config.SaveConfig();

				Logger.Log(
					"Audio services has been disabled since YouTube services are disabled!\nEnable them via the config menu.",
					LogVerbosity.Error);
				return;
			}

#if WINDOWS
			ILibsPreparer libsPreparer = new WindowsLibPreparer();
#elif LINUX
			ILibsPreparer libsPreparer = new LinuxLibPreparer();
#else
			ILibsPreparer libsPreparer = new MacOSLibPreparer();
#endif
			if (forceRedownload)
				UpdatedMusicServiceFiles(libsPreparer);

			else if (!libsPreparer.CheckLibFiles()) 
				UpdatedMusicServiceFiles(libsPreparer);
			
			if (Config.bot.AudioSettings.AudioServicesEnabled)
				Logger.Log("Audio services are ready!", LogVerbosity.Music);
		}

		/// <summary>
		/// Updates all files related to audio
		/// </summary>
		public static void UpdatedMusicServiceFiles(ILibsPreparer preparer)
		{
			Logger.Log("Downloading required files for audio services...");

			//If the temp directory doesn't exist, create a new one.
			if (!Directory.Exists("Temp/")) Directory.CreateDirectory("Temp/");

			//If the external directory doesn't exist, create it
			if (!Directory.Exists(Config.bot.AudioSettings.ExternalDirectory))
				Directory.CreateDirectory(Config.bot.AudioSettings.ExternalDirectory);

			//We get a json file that tells us where to download other files from
			string json = WebUtils.DownloadString(AudioLibFileJsonUrl);
			List<AudioExternalLibFiles> listOfLibsFilesForOs =
				JsonConvert.DeserializeObject<List<AudioExternalLibFiles>>(json);

			preparer.DownloadFiles(GetUrlsFromOs(listOfLibsFilesForOs));
			Config.SaveConfig();

			Logger.Log("Done! All files needed for audio service are ready!", LogVerbosity.Music);
		}

		private static AudioExternalLibFiles GetUrlsFromOs(IEnumerable<AudioExternalLibFiles> audioExternalLibFiles)
		{
#if WINDOWS
			string osPlatform = "Windows";
#elif LINUX
			string osPlatform = "Linux";
#else
			string osPlatform = "MacOS";
#endif
			IEnumerable<AudioExternalLibFiles> result = from a in audioExternalLibFiles
				where a.OsPlatform == osPlatform
				select a;

			return result.FirstOrDefault();
		}
	}
}