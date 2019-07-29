namespace Pootis_Bot.Structs
{
	public enum ErrorReason { NoAPIKey, HTTPError, Error }

	public struct GiphySearchResult
	{
		public bool IsSuccessfull { get; set; }
		public ErrorReason ErrorReason { get; set; }
		public GiphyData Data { get; set; }
	}
}
