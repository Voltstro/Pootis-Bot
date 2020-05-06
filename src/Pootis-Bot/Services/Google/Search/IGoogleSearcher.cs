using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pootis_Bot.Services.Google.Search
{
	public interface IGoogleSearcher
	{
		/// <summary>
		/// Searches Google
		/// </summary>
		/// <param name="search"></param>
		/// <returns></returns>
		public Task<List<GoogleSearch>> SearchGoogle(string search);
	}
}