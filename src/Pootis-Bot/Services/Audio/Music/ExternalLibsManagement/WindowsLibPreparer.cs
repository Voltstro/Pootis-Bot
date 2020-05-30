#if WINDOWS

using System;
using System.IO;
using System.IO.Compression;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Helpers;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.Audio.Music.ExternalLibsManagement
{
	public class WindowsLibPreparer : ILibsPreparer
	{
		public bool CheckLibFiles()
		{
			string externalDirectory = Config.bot.AudioSettings.ExternalDirectory;

			//If either ffmpeg, opus or libsodium doesn't exist, we need to download them
			return File.Exists($"{externalDirectory}ffmpeg.exe") && File.Exists("opus.dll") &&
			       File.Exists("libsodium.dll");
		}

		public void DownloadFiles(AudioExternalLibFiles libsUrls)
		{
			try
			{
				Logger.Log("Downloading files for Windows...");

				//Download all audio service files for Windows
				Logger.Log($"Downloading ffmpeg from {libsUrls.FfmpegDownloadUrl}");
				WebUtils.DownloadFileAsync(libsUrls.FfmpegDownloadUrl, "Temp/ffmpeg.zip").GetAwaiter().GetResult();
				Logger.Log($"Downloading needed DLLs from {libsUrls.LibsDownloadUrl}");
				WebUtils.DownloadFileAsync(libsUrls.LibsDownloadUrl, "Temp/dlls.zip").GetAwaiter().GetResult();

				//Extract required files
				Logger.Log("Extracting files...");
				ZipFile.ExtractToDirectory("Temp/dlls.zip", "./", true);
				ZipFile.ExtractToDirectory("Temp/ffmpeg.zip", "Temp/ffmpeg/", true);

				//Copy the needed parts of ffmpeg to the right directory
				Logger.Log("Setting up ffmpeg");
				Global.DirectoryCopy("Temp/ffmpeg/", Config.bot.AudioSettings.ExternalDirectory, true);

				//Delete unnecessary files
				Logger.Log("Cleaning up...");
				File.Delete("Temp/dlls.zip");
				File.Delete("Temp/ffmpeg.zip");
				Directory.Delete("Temp/ffmpeg/", true);
			}
			catch (Exception ex)
			{
#if DEBUG
				Logger.Log(
					$"An error occured while preparing music services: {ex}\nMusic services has now been disabled!",
					LogVerbosity.Error);
#else
				Logger.Log($"An error occured while preparing music services: {ex.Message}\nMusic services has now been disabled!", LogVerbosity.Error);
#endif

				Config.bot.AudioSettings.AudioServicesEnabled = false;
				Config.SaveConfig();
			}
		}

		public void DeleteFiles()
		{
			string externalDir = Config.bot.AudioSettings.ExternalDirectory;

			//Delete ffmpeg
			if(File.Exists($"{externalDir}ffmpeg.exe"))
				File.Delete($"{externalDir}ffmpeg.exe");

			//Delete dlls
			if(File.Exists("opus.dll"))
				File.Delete("opus.dll");

			if(File.Exists("libsodium.dll"))
				File.Delete("libsodium.dll");
		}
	}
}

#endif