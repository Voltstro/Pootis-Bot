using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pootis_Bot.Core;

namespace Pootis_Bot.Helpers
{
	/// <summary>
	/// Provides some basic functions for interacting with the <see cref="Global.HttpClient"/>, as well as some other web
	/// related functions
	/// </summary>
	public static class WebUtils
	{
		#region Other Web Related methods

		/// <summary>
		/// Checks if a string is a URL
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static bool IsStringValidUrl(string url)
		{
			const string pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
			Regex rgx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			return rgx.IsMatch(url);
		}

		#endregion

		#region HttpClient Methods

		/// <summary>
		/// Downloads a file from the internet
		/// </summary>
		/// <param name="url">The url of the file you want to download</param>
		/// <param name="filePath">The full file path (including name) of where you want to place the downloaded file</param>
		/// <returns></returns>
		public static async Task DownloadFileAsync(string url, string filePath)
		{
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
			await using Stream contentStream =
					await (await Global.HttpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
				stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

			await contentStream.CopyToAsync(stream);
		}

		/// <summary>
		/// Download and returns a string
		/// </summary>
		/// <param name="url">The url of the string you want to download</param>
		/// <returns></returns>
		public static string DownloadString(string url)
		{
			using HttpResponseMessage response = Global.HttpClient.GetAsync(url).Result;
			using HttpContent content = response.Content;

			return content.ReadAsStringAsync().Result;
		}

		/// <summary>
		/// Downloads and returns a string (but with a header)
		/// </summary>
		/// <param name="url">The url of the string you want to download</param>
		/// <param name="scheme">The name of scheme of the header</param>
		/// <param name="parameter">The value you want that header to have</param>
		/// <returns></returns>
		public static string DownloadString(string url, string scheme, string parameter)
		{
			using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

			requestMessage.Headers.Add(scheme, parameter);
			using HttpResponseMessage response = Global.HttpClient.SendAsync(requestMessage).Result;
			using HttpContent content = response.Content;

			return content.ReadAsStringAsync().Result;
		}

		#endregion
	}
}