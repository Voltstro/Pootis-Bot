namespace Pootis_Bot.Structs.Giphy
{
	/// <summary>
	/// Giphy search result data
	/// </summary>
	public struct GiphyData
	{
		/// <summary>
		/// The URL to the original image
		/// </summary>
		public string GifUrl { get; set; }

		/// <summary>
		/// The title of the gif
		/// </summary>
		public string GifTitle { get; set; }

		/// <summary>
		/// The author of the gif
		/// </summary>
		public string GifAuthor { get; set; }

		/// <summary>
		/// The link to the Giphy post
		/// </summary>
		public string GifLink { get; set; }
	}
}