using System.Threading;
using Discord.Audio;
using Discord.WebSocket;
using Pootis_Bot.Services.Audio.Music;
using Pootis_Bot.Services.Audio.Music.Playback;

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
		/// The active <see cref="StandardMusicDownloader"/>
		/// </summary>
		public StandardMusicDownloader Downloader { get; set; }

		/// <summary>
		/// Active MusicPlayback, where the actual music stream comes from
		/// </summary>
		public IMusicPlaybackInterface MusicPlayback { get; set; }

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
		/// Active <see cref="CancellationTokenSource"/>
		/// </summary>
		public CancellationTokenSource CancellationSource { get; set; }
	}
}