using System.Diagnostics;
using System.Threading;
using Discord.Audio;
using Discord.WebSocket;

namespace Pootis_Bot.Entities
{
    public class GlobalServerMusicItem
    {
        public ulong GuildID { get; set; }

        public bool IsPlaying { get; set; }

        public bool IsExit { get; set; }

        public CancellationTokenSource CancellationSource { get; set; }
        public IAudioClient AudioClient { get; set; }
        public SocketVoiceChannel AudioChannel { get; set; }
        public ISocketMessageChannel StartChannel { get; set; }
        public AudioOutStream Discord { get; set; } 
        public Process Ffmpeg { get; set; }
    }
}