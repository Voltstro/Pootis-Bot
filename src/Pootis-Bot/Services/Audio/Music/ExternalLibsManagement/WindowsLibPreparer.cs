#if WINDOWS

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
			return File.Exists($"{externalDirectory}ffmpeg.exe") && File.Exists("opus.dll") && File.Exists("libsodium.dll");
		}

		public void DownloadFiles(AudioExternalLibFiles libsUrls)
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
			Global.DirectoryCopy("Temp/ffmpeg/ffmpeg-latest-win64-static/bin/", "External/", true);
			File.Copy("Temp/ffmpeg/ffmpeg-latest-win64-static/LICENSE.txt", "External/ffmpeg-license.txt", true);

			//Delete unnecessary files
			Logger.Log("Cleaning up...");
			File.Delete("Temp/dlls.zip");
			File.Delete("Temp/ffmpeg.zip");
			Directory.Delete("temp/ffmpeg", true);
			Config.bot.AudioSettings.FfmpegLocation = "External/ffmpeg.exe";
		}
	}
}

#endif