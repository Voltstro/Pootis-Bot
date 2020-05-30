using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.Audio.Music.ExternalLibsManagement
{
	public interface ILibsPreparer
	{
		/// <summary>
		/// Checks to make sure all require libs needed for playing music exists
		/// </summary>
		/// <returns></returns>
		public bool CheckLibFiles();

		/// <summary>
		/// Downloads lib files
		/// </summary>
		/// <param name="libsUrls"></param>
		public void DownloadFiles(AudioExternalLibFiles libsUrls);

		/// <summary>
		/// Deletes all the lib files
		/// </summary>
		public void DeleteFiles();
	}
}