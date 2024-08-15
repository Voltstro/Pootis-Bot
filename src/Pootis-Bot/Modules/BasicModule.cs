using System.Threading.Tasks;
using Discord.Interactions;

namespace Pootis_Bot.Modules;

public class BasicModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Gets the ping of the bot")]
    public async Task Ping()
    {
        await RespondAsync($"Ping Pong! {Context.Client.Latency}ms.");
    }
}