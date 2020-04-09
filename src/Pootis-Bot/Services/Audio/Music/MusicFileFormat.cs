namespace Pootis_Bot.Services.Audio.Music
{
	public enum MusicFileFormat
	{
		Mp3
	}

	public static class MusicFileFormatExtensions
	{
		public static string GetFormatExtension(this MusicFileFormat fileFormat)
		{
			string fileFormatInString = fileFormat.ToString();
			return fileFormatInString.ToLowerInvariant();
		}
	}
}