using System.IO;
using System.IO.Compression;
using Pootis_Bot.Core;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Services.Audio
{
	public class AudioDownloadServiceFiles
	{
		/// <summary>
		/// Downloads files for Windows
		/// </summary>
		/// <param name="windowsLibFile"></param>
		public static void PrepareWindowFiles(LibFile windowsLibFile)
		{
			Global.Log("Downloading files for Windows...");

			//Download all audio service files for Windows
			Global.Log("Downloading FFMPEG...");
			WebUtils.DownloadFileAsync(windowsLibFile.FfMpegDownloadUrl, "Temp/ffmpeg.zip").GetAwaiter().GetResult();
			Global.Log("Downloading libsodium.dll and opus.dll...");
			WebUtils.DownloadFileAsync(windowsLibFile.LibsDownloadUrl, "Temp/AudioDlls.zip").GetAwaiter().GetResult();

			//Extract required files
			Global.Log("Extracting files...");
			ZipFile.ExtractToDirectory("Temp/AudioDlls.zip", "./", true);
			ZipFile.ExtractToDirectory("Temp/ffmpeg.zip", "Temp/ffmpeg/", true);

			//Copy the needed parts of ffmpeg to the right directory
			Global.Log("Setting up FFMPEG");
			Global.DirectoryCopy("Temp/ffmpeg/ffmpeg-latest-win64-static/bin/", "External/", true);
			File.Copy("Temp/ffmpeg/ffmpeg-latest-win64-static/LICENSE.txt", "External/ffmpeg-license.txt", true);

			//Delete unnecessary files
			Global.Log("Cleaning up...");
			File.Delete("Temp/AudioDlls.zip");
			File.Delete("Temp/ffmpeg.zip");
			Directory.Delete("temp/ffmpeg", true);

			Config.SaveConfig();
		}

		public class LibFile
		{
			public string OsPlatform { get; set; }
			public string YoutubeDlDownloadUrl { get; set; }
			public string FfMpegDownloadUrl { get; set; }
			public string PythonDownloadUrl { get; set; }
			public string LibsDownloadUrl { get; set; }
		}
	}
}
