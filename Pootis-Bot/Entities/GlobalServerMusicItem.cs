using Discord.Audio;
using System.Collections.Generic;

namespace Pootis_Bot.Entities
{
    public class GlobalServerMusicItem
    {
        public ulong GuildID { get; set; }

        public bool IsPlaying { get; set; }

        public bool IsExit { get; set; }

        public IAudioClient AudioClient { get; set; }
    }
}