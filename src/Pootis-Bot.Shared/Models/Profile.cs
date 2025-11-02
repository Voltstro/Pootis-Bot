using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pootis_Bot.Shared.Models;

[Table("profile")]
public class Profile
{
    /// <summary>
    ///     Primary key
    /// </summary>
    [Key]
    public Guid Id { get; init; }
    
    /// <summary>
    ///     Discord ID
    /// </summary>
    public ulong DiscordId { get; set; }
    
    /// <summary>
    ///     Creates time for this profile
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    ///     Updated time for this profile
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    ///     Amount of XP this user has
    /// </summary>
    public uint Xp { get; set; }
    
    /// <summary>
    ///     User's last time they got XP for a message they sent
    /// </summary>
    public DateTime? LastXpMessageTime { get; set; }
    
    /// <summary>
    ///     User's profile message
    /// </summary>
    public string? ProfileMessage { get; set; }
    
    public uint LevelNumber => (uint) Math.Sqrt(Xp / 30f);
}