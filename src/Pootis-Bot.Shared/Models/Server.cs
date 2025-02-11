using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pootis_Bot.Shared.Models;

[Table("server")]
public class Server
{
    /// <summary>
    ///     Primary key for this server
    /// </summary>
    [Key]
    public Guid Id { get; init; }
    
    /// <summary>
    ///     Discord's ID for this server/guild
    /// </summary>
    public ulong DiscordId { get; set; }
    
    /// <summary>
    ///     Creates time for this server
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    ///     Updated time for this server
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    #region Welcome/Goobye Messages

    public ulong? WelcomeGoodbyeChannelId { get; set; }
    
    public bool WelcomeMessageEnabled { get; set; }
    public bool GoodbyeMessageEnabled { get; set; }

    #endregion

    #region Rule Reaction

    public ulong? RuleReactionChannelId { get; set; }
    
    public ulong? RuleReactionMessageId { get; set; }
    
    public string? RuleReactionEmoji { get; set; }
    
    public bool RuleReactionEnabled { get; set; }

    #endregion
}