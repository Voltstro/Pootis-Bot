using Discord;

namespace Pootis_Bot.Core;

/// <summary>
///     Pootis-Bot's general config
/// </summary>
public class PootisBotConfig
{
    /// <summary>
    ///     Discord bot token
    /// </summary>
    public string BotToken { get; init; }
    
    /// <summary>
    ///     Name of the bot will use
    /// </summary>
    public string BotName { get; init; }
    
    /// <summary>
    ///     Required <see cref="GatewayIntents" /> for the discord client
    /// </summary>
    public GatewayIntents GatewayIntents { get; init; } = GatewayIntents.AllUnprivileged |
                                                                  GatewayIntents.GuildMembers |
                                                                  GatewayIntents.GuildPresences |
                                                                  GatewayIntents.MessageContent;
    
    public ulong? TestGuildId { get; init; }
}