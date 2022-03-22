using Discord;
using Newtonsoft.Json;
using Pootis_Bot.Config;
using Pootis_Bot.Console.ConfigMenus;

namespace Pootis_Bot.Core;

/// <summary>
///     Config used for core stuff in Pootis-Bot
/// </summary>
[MenuItemFormat("Bot Config")]
public class BotConfig : Config<BotConfig>
{
    /// <summary>
    ///     The token used to connect to Discord
    /// </summary>
    [MenuItemFormat("Token")]
    [JsonProperty]
    public string BotToken { get; internal set; } = string.Empty;

    /// <summary>
    ///     The name that is used for display purposes
    /// </summary>
    [MenuItemFormat("Display Name")]
    [JsonProperty]
    public string BotName { get; internal set; } = "Pootis-Bot";

    /// <summary>
    ///     Required <see cref="GatewayIntents" /> for the discord client
    /// </summary>
    [DontShowItem]
    [JsonProperty]
    public GatewayIntents GatewayIntents { get; internal set; } = GatewayIntents.AllUnprivileged |
                                                                  GatewayIntents.GuildMembers |
                                                                  GatewayIntents.GuildPresences;

#if DEBUG
    [DontShowItem] [JsonProperty] public ulong TestingGuildId { get; internal set; }
#endif
}