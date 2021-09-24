namespace Pootis_Bot.Module.WelcomeMessage.Entities
{
    public class WelcomeMessageServer
    {
        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }
        
        public string WelcomeMessage { get; set; }
        
        public bool WelcomeMessageEnabled { get; set; }
        
        public string GoodbyeMessage { get; set; }
        
        public bool GoodbyeMessageEnabled { get; set; }

        public void Disable()
        {
            WelcomeMessageEnabled = false;
            GoodbyeMessageEnabled = false;
        }
    }
}