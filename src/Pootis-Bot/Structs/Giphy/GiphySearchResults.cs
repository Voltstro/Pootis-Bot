namespace Pootis_Bot.Structs.Giphy
{
	public enum ErrorReason
	{
		/// <summary>
		/// No API key provided
		/// </summary>
		NoApiKey,

		/// <summary>
		/// A general error
		/// </summary>
		Error
	}

	public struct GiphySearchResult
	{
		/// <summary>
		/// Was the search successful?
		/// </summary>
		public bool IsSuccessful { get; set; }

		/// <summary>
		/// If the search wasn't successful, why?
		/// </summary>
		public ErrorReason ErrorReason { get; set; }

		/// <summary>
		/// The search result, only if <see cref="IsSuccessful"/> is true.
		/// </summary>
		public GiphyData Data { get; set; }
	}
}