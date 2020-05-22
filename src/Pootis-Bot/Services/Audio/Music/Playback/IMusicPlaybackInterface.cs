using System.Threading;
using System.Threading.Tasks;

namespace Pootis_Bot.Services.Audio.Music.Playback
{
	public interface IMusicPlaybackInterface
	{
		/// <summary>
		/// Reads a sequence of bytes from the current music stream
		/// </summary>
		/// <returns></returns>
		public Task<int> ReadAudioStream(byte[] buffer, int count, CancellationToken cancellationToken);

		/// <summary>
		/// Clears all buffers for the stream
		/// </summary>
		/// <returns></returns>
		public Task Flush();

		/// <summary>
		/// Skips a certain amount of seconds
		/// </summary>
		/// <param name="seconds"></param>
		public void Skip(int seconds);

		/// <summary>
		/// Ends the current audio stream
		/// </summary>
		public void EndAudioStream();
	}
}