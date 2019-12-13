using System;

namespace Pootis_Bot.Structs.Config
{
	public struct ConfigAudio
	{
		/// <summary>
		/// Are the audio services enabled
		/// </summary>
		public bool AudioServicesEnabled { get; set; }

		/// <summary>
		/// Should we log when a song starts or stops on a guild
		/// </summary>
		public bool LogPlayStopSongToConsole { get; set; }

		/// <summary>
		/// The location, or command for ffmpeg
		/// </summary>
		public string FfmpegLocation { get; set; }

		/// <summary>
		/// Max video time
		/// </summary>
		public TimeSpan MaxVideoTime { get; set; }
	}
}