using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules.Account
{
	public class AccountDataManagement : ModuleBase<SocketCommandContext>
	{
		[Command("requestdata")]
		[Alias("getdata", "mydata")]
		[Summary("Gets your profile data and DMs you it in a JSON file.")]
		public async Task RequestData()
		{
			await Context.Channel.SendMessageAsync(
				"Hang on, I will DM you the JSON file once I have collected all of your account data.");

			//Create the temp directory if it doesn't exist
			if (!Directory.Exists("temp/"))
				Directory.CreateDirectory("temp/");

			//Get the user account in a single json file
			string json = JsonConvert.SerializeObject(UserAccounts.GetAccount((SocketGuildUser) Context.User), Formatting.Indented);
			File.WriteAllText($"temp/{Context.User.Id}.json", json);

			//Get the user's dm and send the file
			IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
			await dm.SendFileAsync($"temp/{Context.User.Id}.json", "Here is your user data, all in one JSON file!");

			//Delete the file
			File.Delete($"temp/{Context.User.Id}.json");
		}
	}
}
