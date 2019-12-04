using System.Diagnostics;
using Discord.Audio;
using Discord.WebSocket;

namespace Pootis_Bot.Entities
{
	public class ServerMusicItem
	{
		/// <summary>
		///     What is the ID of this Guild
		/// </summary>
		public ulong GuildId { get; set; }

		/// <summary>
		///     Are we playing music right now
		/// </summary>
		public bool IsPlaying { get; set; }

		/// <summary>
		///     Are we ending the current song
		/// </summary>
		public bool IsExit { get; set; }

		/// <summary>
		///     Audio client
		/// </summary>
		public IAudioClient AudioClient { get; set; }

		/// <summary>
		///     Audio channel
		/// </summary>
		public SocketVoiceChannel AudioChannel { get; set; }

		/// <summary>
		///     The text message channel we started with
		/// </summary>
		public ISocketMessageChannel StartChannel { get; set; }

		/// <summary>
		///     Discord stream
		/// </summary>
		public AudioOutStream Discord { get; set; }

		/// <summary>
		///     FFMpeg process
		/// </summary>
		public Process FfMpeg { get; set; }
	}
}