using System;

namespace Pootis_Bot.Services.Google.YouTube
{
	/// <summary>
	/// A YouTube Video
	/// </summary>
	public class YouTubeVideo
	{
		/// <summary>
		/// The ID of the YouTube video
		/// </summary>
		public string VideoId { get; set; }

		/// <summary>
		/// The title of the YouTube video
		/// </summary>
		public string VideoTitle { get; set; }

		/// <summary>
		/// The person who made this video
		/// </summary>
		public string VideoAuthor { get; set; }

		/// <summary>
		/// Duration of the video
		/// </summary>
		public TimeSpan VideoDuration { get; set; }
	}
}