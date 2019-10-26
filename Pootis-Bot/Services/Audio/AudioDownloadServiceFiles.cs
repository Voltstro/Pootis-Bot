using System.IO;
using System.IO.Compression;
using System.Net;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services.Audio
{
	public class AudioDownloadServiceFiles
	{
		/// <summary>
		/// Downloads files for Windows
		/// </summary>
		/// <param name="windowsLibFile"></param>
		/// <param name="webClient"></param>
		public static void PrepareWindowFiles(LibFile windowsLibFile, WebClient webClient)
		{
			Global.Log("Downloading files for Windows...");

			//Download all audio service files for Windows
			Global.Log("Downloading YouTube-Dl for Windows...");
			webClient.DownloadFile(windowsLibFile.YoutubeDlDownloadUrl, "External/youtube-dl.exe");
			Global.Log("Downloading FFMPEG...");
			webClient.DownloadFile(windowsLibFile.FfMpegDownloadUrl, "Temp/ffmpeg.zip");
			Global.Log("Downloading libsodium.dll and opus.dll...");
			webClient.DownloadFile(windowsLibFile.LibsDownloadUrl, "Temp/AudioDlls.zip");

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

			Config.bot.AudioSettings.InitialApplication = "External\\youtube-dl.exe";
			Config.bot.AudioSettings.PythonArguments = "";
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
