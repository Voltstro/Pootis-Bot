using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Pootis_Bot.Core;

namespace Pootis_Bot.Helpers
{
	public static class WebUtils
	{
		public static async Task DownloadFileAsync(string url, string fileName)
		{
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
			await using Stream contentStream =
					await (await Global.HttpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
				stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

			await contentStream.CopyToAsync(stream);
		}

		public static string DownloadString(string url)
		{
			using HttpResponseMessage response = Global.HttpClient.GetAsync(url).Result;
			using HttpContent content = response.Content;

			return content.ReadAsStringAsync().Result;
		}
	}
}