using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.Audio.Music.ExternalLibsManagement
{
	public interface ILibsPreparer
	{
		public bool CheckLibFiles();

		public void DownloadFiles(AudioExternalLibFiles libsUrls);
	}
}