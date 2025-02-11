using System;
using Discord;
using Victoria;

namespace Pootis_Bot.Core;

/// <summary>
///     Pootis-Bot's general config
/// </summary>
public class PootisBotConfig
{
    public const string PootisBotConfigKey = "Config";
    
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
    
    //Audio Settings
    
    public bool EnableAudioServices { get; set; }
    
    /// <summary>
    ///     Configuration for Victoria
    /// </summary>
    public Configuration? VictoriaConfig { get; init; }
    
    //Xp Settings
    
    public TimeSpan XpGiveCooldown { get; init; } = new(0, 0, 15);
    public uint XpGiveAmount { get; init; } = 15;
}