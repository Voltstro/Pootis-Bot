namespace Pootis_Bot.Services.Audio.Music
{
	/// <summary>
	/// The music file container
	/// </summary>
	public enum MusicFileFormat
	{
		Mp3
	}

	public static class MusicFileFormatExtensions
	{
		/// <summary>
		/// Converts the <see cref="MusicFileFormat"/> enum to a <see cref="string"/>
		/// </summary>
		/// <param name="fileFormat"></param>
		/// <returns></returns>
		public static string GetFormatExtension(this MusicFileFormat fileFormat)
		{
			string fileFormatInString = fileFormat.ToString();
			return fileFormatInString.ToLowerInvariant();
		}
	}
}