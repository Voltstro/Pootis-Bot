using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pootis_Bot.Services.Google.YouTube
{
	public interface IYouTubeSearcher
	{
		/// <summary>
		/// Searches YouTube for a query
		/// <para>Will return null if no video were found</para>
		/// </summary>
		/// <param name="search"></param>
		/// <returns></returns>
		public Task<IList<YouTubeVideo>> SearchForYouTube(string search);

		/// <summary>
		/// Gets A video
		/// </summary>
		/// <param name="videoId"></param>
		/// <returns></returns>
		public Task<YouTubeVideo> GetVideo(string videoId);
	}
}