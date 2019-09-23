namespace Pootis_Bot.Structs
{
	public enum ErrorReason
	{
		NoApiKey,
		Error
	}

	public struct GiphySearchResult
	{
		public bool IsSuccessful { get; set; }
		public ErrorReason ErrorReason { get; set; }
		public GiphyData Data { get; set; }
	}
}