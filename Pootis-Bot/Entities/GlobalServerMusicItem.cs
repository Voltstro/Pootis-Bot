using System.Diagnostics;
using Discord.Audio;
using Discord.WebSocket;

namespace Pootis_Bot.Entities
{
	public class GlobalServerMusicItem
	{
		/// <summary>
		/// What is the ID of this Guild
		/// </summary>
		public ulong GuildId { get; set; }

		/// <summary>
		/// Are we playing music right now
		/// </summary>
		public bool IsPlaying { get; set; }

		public bool IsExit { get; set; }

		public IAudioClient AudioClient { get; set; }
		public SocketVoiceChannel AudioChannel { get; set; }
		public ISocketMessageChannel StartChannel { get; set; }
		public AudioOutStream Discord { get; set; }
		public Process FfMpeg { get; set; }
	}
}