namespace Pootis_Bot.Structs
{
	/// <summary>
	/// URLs to where to get libs and ffmpeg from
	/// </summary>
	public struct AudioExternalLibFiles
	{
		/// <summary>
		/// The platform these URLs are for
		/// </summary>
		public string OsPlatform { get; set; }

		/// <summary>
		/// The URL to where to get ffmpeg
		/// </summary>
		public string FfmpegDownloadUrl { get; set; }

		/// <summary>
		/// The URL to where to get libs (libsodium, opus)
		/// </summary>
		public string LibsDownloadUrl { get; set; }
	}
}