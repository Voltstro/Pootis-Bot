using System.Diagnostics;
using Discord.Audio;
using Discord.WebSocket;
using Pootis_Bot.Services.Audio;

namespace Pootis_Bot.Entities
{
	public class ServerMusicItem
	{
		/// <summary>
		/// What is the ID of this Guild
		/// </summary>
		public ulong GuildId { get; set; }

		/// <summary>
		/// Are we playing music right now
		/// </summary>
		public bool IsPlaying { get; set; }

		/// <summary>
		/// Are we ending the current song
		/// </summary>
		public bool IsExit { get; set; }

		/// <summary>
		/// The active <see cref="AudioDownloadMusicFiles"/>
		/// </summary>
		public AudioDownloadMusicFiles AudioMusicFilesDownloader { get; set; }

		/// <summary>
		/// The current <see cref="IAudioClient"/>
		/// </summary>
		public IAudioClient AudioClient { get; set; }

		/// <summary>
		/// The active <see cref="SocketVoiceChannel"/> the bot is playing in
		/// </summary>
		public SocketVoiceChannel AudioChannel { get; set; }

		/// <summary>
		/// The <see cref="ISocketMessageChannel"/> that the first command was executed in
		/// </summary>
		public ISocketMessageChannel StartChannel { get; set; }

		/// <summary>
		/// THe <see cref="AudioOutStream"/>
		/// </summary>
		public AudioOutStream Discord { get; set; }

		/// <summary>
		/// The ffmpeg <see cref="Process"/>
		/// </summary>
		public Process FfMpeg { get; set; }
	}
}