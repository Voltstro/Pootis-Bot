using Newtonsoft.Json;
using Pootis_Bot.Helpers;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Services.Fun
{
	public class RandomPersonGenerator
	{
		public static RandomPersonResults GenerateRandomPerson()
		{
			string json = WebUtils.DownloadString("https://randomuser.me/api/");

			dynamic data = JsonConvert.DeserializeObject<dynamic>(json);

			RandomPersonResults results = new RandomPersonResults
			{
				PersonGender = data.results[0].gender.ToString(),
				PersonFirstName = data.results[0].name.first.ToString(),
				PersonLastName = data.results[0].name.last.ToString(),
				PersonTitle = data.results[0].name.title.ToString(),
				City = data.results[0].location.city.ToString(),
				State = data.results[0].location.state.ToString(),
				Country = data.results[0].location.country.ToString(),
				CountryCode = data.results[0].nat.ToString(),
				PersonPicture = data.results[0].picture.large.ToString()
			};

			return results;
		}
	}
}