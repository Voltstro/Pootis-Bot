using System.Threading.Tasks;
using Discord.Interactions;
using Pootis_Bot.Config;
using Pootis_Bot.Core;

namespace Pootis_Bot.Module.Basic;

public class BasicCommandsInteraction : InteractionModuleBase<SocketInteractionContext>
{
    private string displayName;

    public BasicCommandsInteraction()
    {
        BotConfig config = Config<BotConfig>.Instance;
        displayName = config.BotName;
        config.Saved += () => displayName = config.BotName;
    }

    [SlashCommand("hello", "Provides about info")]
    public async Task Hello()
    {
        await RespondAsync("Hello!");
    }
}