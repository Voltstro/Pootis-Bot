using System.Threading.Tasks;

namespace Pootis_Bot.Services.Audio.Music.Conversion
{
	public interface IAudioConverter
	{
		public Task<string> ConvertFileToAudio(string originalLocation, string location, bool deleteOriginal = true,
			MusicFileFormat musicFileFormat = MusicFileFormat.Mp3);
	}
}