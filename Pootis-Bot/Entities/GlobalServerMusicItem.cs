using Discord.Audio;

namespace Pootis_Bot.Entities
{
    public class GlobalServerMusicItem
    {
        public ulong GuildID { get; set; }

        public bool IsPlaying { get; set; }

        public IAudioClient AudioClient { get; set; }
    }
}