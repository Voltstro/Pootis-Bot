using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Preconditions;

namespace Pootis_Bot.Modules.Account
{
	public class AccountDataManagement : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Voltstro
		// Description      - Allows users to manage their profile data
		// Contributors     - Voltstro, HelloHowIsItGoing

		[Command("requestdata")]
		[Alias("getdata", "mydata")]
		[Summary("Gets your profile data and DMs you it in a JSON file.")]
		[Cooldown(150)]
		public async Task RequestData()
		{
			await Context.Channel.SendMessageAsync(
				"Hang on, I will DM you the JSON file once I have collected all of your account data.");

			//Create the temp directory if it doesn't exist
			if (!Directory.Exists("temp/"))
				Directory.CreateDirectory("temp/");

			//Get the user account in a single json file
			string json = JsonConvert.SerializeObject(UserAccountsManager.GetAccount((SocketGuildUser) Context.User),
				Formatting.Indented);
			File.WriteAllText($"temp/{Context.User.Id}.json", json);

			//Get the user's dm and send the file
			IDMChannel dm = await Context.User.CreateDMChannelAsync();
			await dm.SendFileAsync($"temp/{Context.User.Id}.json", "Here is your user data, all in one JSON file!");

			//Delete the file
			File.Delete($"temp/{Context.User.Id}.json");
		}

		[Command("resetprofile")]
		[Alias("Resets your profile data. For more info do `resetprofile info`")]
		[Cooldown(5)]
		public async Task ResetProfile(string confirm = "")
		{
			if (confirm.ToLower() == "info")
			{
				await Context.Channel.SendMessageAsync(
					"Resting your profile will reset your XP back down to 0 and reset your profile message.\n**THIS WILL NOT RESET SERVER DATA ASSOCIATED WITH YOUR PROFILE!**\n\nTo confirm you want to reset your profile data do `resetprofile yes`.");

				return;
			}

			if (confirm.ToLower() != "yes")
			{
				await Context.Channel.SendMessageAsync(
					"For more info on profile resets do `resetprofile info`. To confirm you want to reset your profile data do `resetprofile yes`.");
				return;
			}


			UserAccount user = UserAccountsManager.GetAccount((SocketGuildUser) Context.User);
			user.ProfileMsg = "";
			user.Xp = 0;

			UserAccountsManager.SaveAccounts();

			await Context.Channel.SendMessageAsync("Your profile data has been reset!");
		}
	}
}