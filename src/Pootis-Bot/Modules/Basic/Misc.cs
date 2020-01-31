using System;
using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Services;

namespace Pootis_Bot.Modules.Basic
{
	public class Misc : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Misc commands
		// Contributors     - Creepysin, 

		[Command("pick")]
		[Summary("Picks between two or more things. Separate each choice with a |.")]
		public async Task PickOne([Remainder] string[] options)
		{
			Random r = new Random();
			string selection = options[r.Next(0, options.Length)];
			await Context.Channel.SendMessageAsync($"I choose... **{selection}**.");
		}

		[Command("roll")]
		[Summary("Roles between 0 and 50 or between two custom numbers")]
		public async Task Roll(int min = 0, int max = 50)
		{
			Random r = new Random();
			int random = r.Next(min, max);
			await Context.Channel.SendMessageAsync("The number was: " + random);
		}

		[Command("reminds", RunMode = RunMode.Async)]
		[Summary("Reminds you, duh (In Seconds)")]
		[Alias("res")]
		public async Task Remind(int seconds, [Remainder] string remindMsg)
		{
			await Context.Channel.SendMessageAsync(
				$"Ok, I will send you the message '{remindMsg}' in {seconds} seconds.");
			await ReminderService.RemindAsyncSeconds(Context.User, seconds, remindMsg);
		}
	}
}