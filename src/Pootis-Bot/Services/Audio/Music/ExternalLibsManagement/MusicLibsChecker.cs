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
			Logger.Info("Checking music services...");

			//If YouTube services has disabled, we cannot use audio services
			if (!Config.bot.Apis.YouTubeService)
			{
				Config.bot.AudioSettings.AudioServicesEnabled = false;
				Config.SaveConfig();

				Logger.Error(
					"Audio services has been disabled since YouTube services are disabled!\nEnable them via the config menu.");
				return;
			}

			ILibsPreparer libsPreparer = GetLibsPreparer();

			if (forceRedownload)
				UpdatedMusicServiceFiles(libsPreparer);

			else if (!libsPreparer.CheckLibFiles()) 
				UpdatedMusicServiceFiles(libsPreparer);
			
			if (Config.bot.AudioSettings.AudioServicesEnabled)
				Logger.Info("Music services are ready!");
		}

		public static ILibsPreparer GetLibsPreparer()
		{
#if WINDOWS
			return new WindowsLibPreparer();
#elif LINUX
			return new LinuxLibPreparer();
#else
			return new MacOSLibPreparer();
#endif
		}

		/// <summary>
		/// Updates all files related to audio
		/// </summary>
		private static void UpdatedMusicServiceFiles(ILibsPreparer preparer)
		{
			Logger.Info("Downloading required files for the music services...");

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

			Logger.Info("Done! All files needed to play music are ready!");
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