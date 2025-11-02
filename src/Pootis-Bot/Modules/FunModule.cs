using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Pootis_Bot.Shared.Helper;
using WikiDotNet;

namespace Pootis_Bot.Modules;

[Group("", "Fun related commands")]
public class FunModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly WikiSearcher wikiSearcher;
    
    public FunModule(WikiSearcher searcher)
    {
        wikiSearcher = searcher;
    }
    
    [SlashCommand("wiki", "Searches wikipedia")]
    public async Task WikiSearch(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            await RespondAsync(Messages.ValidationFailed(nameof(search), "not empty"));
            return;
        }

        await RespondAsync(Messages.Search("wikipedia"));
        IUserMessage responseAsync = await GetOriginalResponseAsync();
        
        WikiSearchResponse searchResult = await wikiSearcher.SearchAsync(search, new WikiSearchSettings
        {
            ResultLimit = 8,
            BotUserAgent = "PootisBot (https://github.com/Voltstro/Pootis-Bot)"
        });
        
        if (!searchResult.WasSuccessful)
        {
            //TODO: We should read the errors
            await responseAsync.ModifyAsync(x => x.Content = Messages.SearchFailed("wikipedia"));
            return;
        }

        EmbedBuilder embedBuilder = new();
        embedBuilder.WithTitle($"Wikipedia Search Results for `{search}`");
        embedBuilder.WithColor(252, 252, 252);
        embedBuilder.WithTimestamp(searchResult.Timestamp);
        foreach (WikiSearchResult querySearchResult in searchResult.Query.SearchResults)
        {
            embedBuilder.AddField($"{querySearchResult.Title} - ({querySearchResult.ConstantUrl.AbsoluteUri})",
                $"{querySearchResult.Preview}...");
        }
        
        await responseAsync.ModifyAsync(x =>
        {
            x.Content = string.Empty;
            x.Embed = embedBuilder.Build();
        });
    }
}