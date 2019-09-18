using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services.Audio
{
	public static class AudioCheckService
	{
		//Downloads a .json file that has were to get some needed libs from
		private static readonly string audioLibFileJsonUrl = "https://pootis-bot.creepysin.com/download/audiolibfiles.json";

		public static void CheckAudioService()
		{
			if (Config.bot.IsAudioServiceEnabled)
			{
				Global.Log("Checking audio services...", ConsoleColor.Blue);

				//Check to see if all the necessary files are here.
				if (!File.Exists("external/python.exe") || !File.Exists("external/ffmpeg.exe") || !File.Exists("external/ffplay.exe") || !File.Exists("external/ffprobe.exe") || !Directory.Exists("external/youtube_dl"))
				{
					UpdateAudioFiles();
				}

				if (string.IsNullOrWhiteSpace(Config.bot.Apis.ApiYoutubeKey))
				{
					Global.Log("You need to set a YouTube api key! You can get one from https://console.developers.google.com and creating a new project with the YouTube Data API v3", ConsoleColor.Red);
					Config.bot.IsAudioServiceEnabled = false;
					Config.SaveConfig();
					Global.Log("Audio service was disabled!", ConsoleColor.Red);
				}
				else
				{
					if (Config.bot.IsAudioServiceEnabled)
						Global.Log("Audio services are ready", ConsoleColor.Blue);
				}

			}
		}

		public static void UpdateAudioFiles()
		{
			Global.Log("Downloading required files for audio services...", ConsoleColor.Blue);

			//If the temp directory doesn't exist, create a new one.
			if (!Directory.Exists("temp/"))
			{
				Directory.CreateDirectory("temp/");
			}

			//Download files
			using (WebClient client = new WebClient())
			{
				//Get the audiolibfiles.json from the pootis-bot website and deserialize it.
				Global.Log($"Gathering base data from {audioLibFileJsonUrl}", ConsoleColor.Blue);
				string json = client.DownloadString(audioLibFileJsonUrl);
				var data = JsonConvert.DeserializeObject<dynamic>(json);
				Global.Log("Done!", ConsoleColor.Blue);

				Global.Log("----==== Downloading Files ====----", ConsoleColor.Blue);

				//Download the dlls
				Global.Log($"Downloading dlls from {data.AudioDllsUrl.ToString()}", ConsoleColor.Blue);
				client.DownloadFile(data.AudioDllsUrl.ToString(), "temp/audiodlls.zip");
				Global.Log("Done!", ConsoleColor.Blue);

				int windowsIndex = 0;
				int linuxIndex = 0;
				int macOsIndex = 0;

				for (int i = 0; i < data.data.Count; i++)
				{
					if (data.data[i].OsName == "Windows")
						windowsIndex = i;
					else if (data.data[i].OsName == "Linux")
						linuxIndex = i;
					else
						macOsIndex = i;
				}

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true && data.data[windowsIndex].IsOsSupported == true)
				{
					if (Environment.Is64BitProcess)
					{
						//Download ffmpeg 64 bit
						Global.Log($"Downloading ffmpeg from {data.data[windowsIndex].Ffmpeg64Url.ToString()}", ConsoleColor.Blue);
						client.DownloadFile(data.data[windowsIndex].Ffmpeg64Url.ToString(), "temp/ffmpeg-latest.zip");
						Global.Log("Done!", ConsoleColor.Blue);

						//Download ffmpeg 32 bit
						Global.Log($"Downloading python from {data.data[windowsIndex].Python64Url}", ConsoleColor.Blue);
						client.DownloadFile(data.data[windowsIndex].Python64Url.ToString(), "temp/python-embed.zip");
						Global.Log("Done!", ConsoleColor.Blue);
					}
					else
					{
						//Download ffmpeg 32 bit
						Global.Log($"Downloading ffmpeg from {data.data[windowsIndex].Ffmpeg32Url.ToString()}", ConsoleColor.Blue);
						client.DownloadFile(data.data[windowsIndex].Ffmpeg64Url.ToString(), "temp/ffmpeg-latest.zip");
						Global.Log("Done!", ConsoleColor.Blue);

						//Download ffmpeg 32 bit
						Global.Log($"Downloading python from {data.data[windowsIndex].Python32Url}", ConsoleColor.Blue);
						client.DownloadFile(data.data[windowsIndex].Python64Url.ToString(), "temp/python-embed.zip");
						Global.Log("Done!", ConsoleColor.Blue);
					}
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					//Linux audio serivces are not supported
					Global.Log("Linux is not supported for the audio services", ConsoleColor.Blue);
					Config.bot.IsAudioServiceEnabled = false;
					Config.SaveConfig();
					Global.Log("Audio service was disabled!", ConsoleColor.Red);
					return;
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					//OSX audio serivces are not supported
					Global.Log("MACOSX is not supported for the audio services", ConsoleColor.Blue);
					Config.bot.IsAudioServiceEnabled = false;
					Config.SaveConfig();
					Global.Log("Audio service was disabled!", ConsoleColor.Red);
					return;
				}

				Global.Log($"Downloading Youtube-Dl {data.YouTubeDlUrl.ToString()}", ConsoleColor.Blue);
				client.DownloadFile(data.YouTubeDlUrl.ToString(), "temp/youtube-dl.zip");
				Global.Log("Done!", ConsoleColor.Blue);
			}

			//Extract files
			Global.Log("----==== Extracting Files ====----", ConsoleColor.Blue);

			//Aduio Dlls
			Global.Log("Extracting audio dlls...", ConsoleColor.Blue);
			ZipFile.ExtractToDirectory("temp/audiodlls.zip", "./", true);
			Global.Log("Done!", ConsoleColor.Blue);

			//FfMpeg
			Global.Log("Extracting ffmpeg...", ConsoleColor.Blue);
			Directory.CreateDirectory("temp/ffmpeg");
			ZipFile.ExtractToDirectory("temp/ffmpeg-latest.zip", "temp/ffmpeg/", true);
			Global.Log("Done!", ConsoleColor.Blue);

			//Python
			Global.Log("Extracting python...", ConsoleColor.Blue);
			Directory.CreateDirectory("temp/python");
			ZipFile.ExtractToDirectory("temp/python-embed.zip", "temp/python/", true);
			Global.Log("Done!", ConsoleColor.Blue);

			//Youtube-dl
			Global.Log("Extracting youtube-dl...", ConsoleColor.Blue);
			Directory.CreateDirectory("temp/youtube-dl");
			ZipFile.ExtractToDirectory("temp/youtube-dl.zip", "temp/youtube-dl/", true);

			Global.Log("Done!", ConsoleColor.Blue);

			//Copy files to there needed directory
			Global.Log("----==== Copying Files ====----", ConsoleColor.Blue);
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true)
			{
				Global.Log("Copying FfMpeg...", ConsoleColor.Blue);
				if (Environment.Is64BitProcess)
				{
					Global.DirectoryCopy("temp/ffmpeg/ffmpeg-latest-win64-static/bin/", "External/", true);
				}
				else
				{
					Global.DirectoryCopy("temp/ffmpeg/ffmpeg-latest-win32-static/bin/", "External/", true);
				}
				Global.Log("Done!", ConsoleColor.Blue);

				Global.Log("Copying python...", ConsoleColor.Blue);
				Global.DirectoryCopy("temp/python/", "External/", true);
				Global.Log("Done!", ConsoleColor.Blue);

				Global.Log("Copying YouTube-Dl", ConsoleColor.Blue);
				Global.DirectoryCopy("temp/youtube-dl/youtube-dl-master/youtube_dl/", "External/youtube_dl/", true);
				Global.Log("Done!", ConsoleColor.Blue);
			}

			Global.Log("Cleaning up...", ConsoleColor.Blue);
			Directory.Delete("temp/", true);
			Global.Log("Done!", ConsoleColor.Blue);
		}
	}
}
