using System.Threading.Tasks;

namespace Pootis_Bot.Services.Audio.Music.Download
{
	public interface IMusicDownloader
	{
		/// <summary>
		/// Downloads a video of from YouTube
		/// <para>Returns the path to where the file was downloaded to, or null if it failed</para>
		/// </summary>
		/// <param name="youTubeVideoId"></param>
		/// <param name="downloadDirectory"></param>
		/// <returns></returns>
		public Task<string> DownloadYouTubeVideo(string youTubeVideoId, string downloadDirectory = "Music/");

		/// <summary>
		/// Gets a YouTube video and returns the title of it, or null if it doesn't exist
		/// </summary>
		/// <param name="youTubeVideoId"></param>
		/// <returns></returns>
		public Task<string> GetYouTubeVideo(string youTubeVideoId);
	}
}