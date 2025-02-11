using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pootis_Bot.Shared.Messages;

namespace Pootis_Bot.Shared.Models;

[Table("server_message")]
public class ServerMessage
{
    [Key]
    public Guid Id { get; init; }
    
    [ForeignKey("Server")]
    public Guid ServerId { get; set; }
    public virtual Server Server { get; set; }
    
    public MessageType Type { get; set; }
    
    public string Message { get; set; }
}