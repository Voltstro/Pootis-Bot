namespace Pootis_Bot.Services.Google.Search
{
	public class GoogleSearch
	{
		public GoogleSearch(string title, string snippet, string link)
		{
			ResultTitle = title;
			ResultSnippet = snippet;
			ResultLink = link;
		}

		public string ResultTitle { get; }
		public string ResultSnippet { get; }
		public string ResultLink { get; }
	}
}