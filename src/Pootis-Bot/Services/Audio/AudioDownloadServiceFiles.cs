using System.IO;
using System.IO.Compression;
using Pootis_Bot.Core;
using Pootis_Bot.Helpers;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.Audio
{
	public class AudioDownloadServiceFiles
	{
#if WINDOWS

		/// <summary>
		/// Downloads files for Windows
		/// </summary>
		/// <param name="downloadUrls"></param>
		public static void DownloadAndPrepareWindowsFiles(AudioExternalLibFiles downloadUrls)
		{
			Global.Log("Downloading files for Windows...");

			//Download all audio service files for Windows
			Global.Log($"Downloading ffmpeg from {downloadUrls.FfmpegDownloadUrl}");
			WebUtils.DownloadFileAsync(downloadUrls.FfmpegDownloadUrl, "Temp/ffmpeg.zip").GetAwaiter().GetResult();
			Global.Log($"Downloading needed DLLs from {downloadUrls.LibsDownloadUrl}");
			WebUtils.DownloadFileAsync(downloadUrls.LibsDownloadUrl, "Temp/dlls.zip").GetAwaiter().GetResult();

			//Extract required files
			Global.Log("Extracting files...");
			ZipFile.ExtractToDirectory("Temp/dlls.zip", "./", true);
			ZipFile.ExtractToDirectory("Temp/ffmpeg.zip", "Temp/ffmpeg/", true);

			//Copy the needed parts of ffmpeg to the right directory
			Global.Log("Setting up ffmpeg");
			Global.DirectoryCopy("Temp/ffmpeg/ffmpeg-latest-win64-static/bin/", "External/", true);
			File.Copy("Temp/ffmpeg/ffmpeg-latest-win64-static/LICENSE.txt", "External/ffmpeg-license.txt", true);

			//Delete unnecessary files
			Global.Log("Cleaning up...");
			File.Delete("Temp/dlls.zip");
			File.Delete("Temp/ffmpeg.zip");
			Directory.Delete("temp/ffmpeg", true);
		}

#elif LINUX
		public static void DownloadAndPrepareLinuxFiles(AudioExternalLibFiles downloadUrls)
		{
			Global.Log("Downloading files for Linux...");

			//Download all audio service files for Linux
			Global.Log($"Downloading ffmpeg from {downloadUrls.FfmpegDownloadUrl}");
			WebUtils.DownloadFileAsync(downloadUrls.FfmpegDownloadUrl, "Temp/ffmpeg.zip").GetAwaiter().GetResult();
			Global.Log($"Downloading needed DLLs from {downloadUrls.LibsDownloadUrl}");
			WebUtils.DownloadFileAsync(downloadUrls.LibsDownloadUrl, "Temp/dlls.zip").GetAwaiter().GetResult();

			//Extract required files
			Global.Log("Extracting files...");
			ZipFile.ExtractToDirectory("Temp/dlls.zip", "./", true);
			ZipFile.ExtractToDirectory("Temp/ffmpeg.zip", "Temp/ffmpeg/", true);

			//Copy the needed parts of ffmpeg to the right directory
			Global.Log("Setting up ffmpeg");
			Global.DirectoryCopy("Temp/ffmpeg/ffmpeg-linux-64/", "External/", true);

			//Delete unnecessary files
			Global.Log("Cleaning up...");
			File.Delete("Temp/dlls.zip");
			File.Delete("Temp/ffmpeg.zip");
			Directory.Delete("Temp/ffmpeg", true);
		}

#elif OSX
#endif
	}
}