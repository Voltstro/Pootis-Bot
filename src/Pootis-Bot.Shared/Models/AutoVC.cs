using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pootis_Bot.Shared.Models;

[Table("autovc")]
public class AutoVC
{
    [Key]
    public Guid Id { get; init; }
    
    [ForeignKey("Server")]
    public Guid ServerId { get; set; }
    public virtual Server Server { get; set; }
    
    public ulong BaseVcChannelId { get; set; }
    
    public string BaseName { get; set; }
    
    public int MaxChannels { get; set; }
    
    public int MaxUsers { get; set; }
    
    /// <summary>
    ///     Creates time for this profile
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    ///     Updated time for this profile
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}