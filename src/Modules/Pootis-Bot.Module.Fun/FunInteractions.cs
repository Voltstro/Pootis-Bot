using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using WikiDotNet;

namespace Pootis_Bot.Module.Fun;

[Group("", "Fun related commands")]
public class FunInteractions : InteractionModuleBase<SocketInteractionContext>
{
    private readonly WikiSearcher wikiSearcher;
    
    public FunInteractions(WikiSearcher searcher)
    {
        wikiSearcher = searcher;
    }
    
    [SlashCommand("wiki", "Searches wikipedia")]
    public async Task WikiSearch(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            await RespondAsync("Search cannot be empty or white space!");
            return;
        }

        await RespondAsync("Searching...");
        
        WikiSearchResponse searchResult = wikiSearcher.Search(search, new WikiSearchSettings
        {
            ResultLimit = 8
        });
        if (!searchResult.WasSuccessful)
        {
            //TODO: We should read the errors
            await RespondAsync("Wiki search was not successful!");
            return;
        }

        EmbedBuilder embedBuilder = new();
        embedBuilder.WithTitle($"Wikipedia Search Results for `{search}`");
        foreach (WikiSearchResult querySearchResult in searchResult.Query.SearchResults)
        {
            embedBuilder.AddField($"{querySearchResult.Title} - ({querySearchResult.ConstantUrl.AbsoluteUri})",
                $"{querySearchResult.Preview}...");
        }

        embedBuilder.WithTimestamp(searchResult.Timestamp);

        IUserMessage responseAsync = await GetOriginalResponseAsync();
        await responseAsync.ModifyAsync(x =>
        {
            x.Content = string.Empty;
            x.Embed = embedBuilder.Build();
        });
    }
}