namespace Pootis_Bot.Discord
{
    /// <summary>
    ///     Abstraction for Discord.Net's Emote and Emoji
    /// </summary>
    public class Emoji
    {
        public string Name { get; set; }
        
        public ulong? Id { get; set; }

        public override string ToString()
        {
            return Id == null ? Name : $"<:{Name}:{Id.Value}>";
        }
    }
}