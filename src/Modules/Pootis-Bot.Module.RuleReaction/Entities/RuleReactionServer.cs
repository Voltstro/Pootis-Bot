namespace Pootis_Bot.Module.RuleReaction.Entities
{
    public class RuleReactionServer
    {
        public ulong GuildId { get; set; }
        
        public bool Enabled { get; set; }
        
        public ulong MessageId { get; set; }
        
        public ulong ChannelId { get; set; }
        
        public ulong RoleId { get; set; }
        
        public string Emoji { get; set; }
    }
}