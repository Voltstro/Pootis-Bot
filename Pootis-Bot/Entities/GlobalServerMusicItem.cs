using Discord.Audio;
using System.Diagnostics;

namespace Pootis_Bot.Entities
{
    public class GlobalServerMusicItem
    {
        public ulong GuildID { get; set; }

        public bool IsPlaying { get; set; }

        public bool IsExit { get; set; }

        public IAudioClient AudioClient { get; set; }
        public AudioOutStream Discord { get; set; } 
        public Process Ffmpeg { get; set; }
    }
}