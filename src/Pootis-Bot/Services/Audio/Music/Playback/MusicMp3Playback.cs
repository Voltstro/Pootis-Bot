using System.Threading;
using System.Threading.Tasks;
using MP3Sharp;

namespace Pootis_Bot.Services.Audio.Music.Playback
{
	public class MusicMp3Playback : IMusicPlaybackInterface
	{
		private readonly MP3Stream reader;

		public MusicMp3Playback(string songLocation)
		{
			reader = new MP3Stream(songLocation);
		}

		public Task<int> ReadAudioStream(byte[] buffer, int count, CancellationToken cancellationToken)
		{
			return reader.ReadAsync(buffer, 0, count, cancellationToken);
		}

		public Task Flush()
		{
			return reader.FlushAsync();
		}

		public void Skip(int seconds)
		{
			
		}

		public void EndAudioStream()
		{
			reader.Flush();
			reader.Dispose();
		}
	}
}