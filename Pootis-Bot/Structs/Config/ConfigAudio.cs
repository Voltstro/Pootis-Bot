using System;

namespace Pootis_Bot.Structs.Config
{
	public struct ConfigAudio
	{
		public bool AudioServicesEnabled { get; set; }
		public bool LogPlayStopSongToConsole { get; set; }

		public TimeSpan MaxVideoTime { get; set; }
	}
}