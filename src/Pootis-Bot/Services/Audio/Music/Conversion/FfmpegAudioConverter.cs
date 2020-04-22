using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;

namespace Pootis_Bot.Services.Audio.Music.Conversion
{
	public class FfmpegAudioConverter : IAudioConverter
	{
		public Task<string> ConvertFileToAudio(string originalLocation, string location, bool deleteOriginal = true,
			MusicFileFormat musicFileFormat = MusicFileFormat.Mp3)
		{
			try
			{
				string fullNewLocation = $"{location}{Path.GetFileName(originalLocation).Replace(Path.GetExtension(originalLocation), "")}.{musicFileFormat.GetFormatExtension()}";

				Logger.Log($"Converting '{originalLocation}' to '{fullNewLocation}'...", LogVerbosity.Debug);

				//Start our ffmpeg process
				Process ffmpeg = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = Config.bot.AudioSettings.FfmpegLocation,
						Arguments = $"-i \"{originalLocation}\" \"{fullNewLocation}\""
					}
				};

				ffmpeg.Start();
				ffmpeg.WaitForExit();

				//Delete our old file
				if (deleteOriginal)
				{
					//if (File.Exists(originalLocation))
					//File.Delete(originalLocation);
					//else //Were the fuck is our fileToConvert then?? This should never happen but it is here anyway
					//return null;
				}

				//So obviously there was an issue converting...
				if (!File.Exists(fullNewLocation))
				{
					Logger.Log("There was an issue converting the file!", LogVerbosity.Debug);
					return null;
				}

				//Ayy, we converted
				Logger.Log($"Successfully converted to '{fullNewLocation}'.", LogVerbosity.Debug);
				return Task.FromResult(fullNewLocation);
			}
			catch (NullReferenceException ex)
			{
#if DEBUG
				Logger.Log($"Null reference exception while trying to convert a song! FFMPEG path could be set incorrectly!\n{ex}", LogVerbosity.Error);
#else
				Logger.Log($"Null refrence exception while trying to convert a song! FFMPEG path could be set incorrectly!\n{ex.Message}", LogVerbosity.Error);
#endif
				return null;
			}
			catch (Exception ex)
			{
#if DEBUG
				Logger.Log(ex.ToString(), LogVerbosity.Error);
#else
				Logger.Log(ex.Message, LogVerbosity.Error);
#endif
				return null;
			}
		}
	}
}