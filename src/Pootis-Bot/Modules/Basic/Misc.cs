using System;
using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Services;
using Pootis_Bot.Services.Voting;

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

		[Command("vote", RunMode = RunMode.Async)]
		[Summary("Starts a vote")]
		public async Task Vote(string title, string description, TimeSpan time, string yesEmoji, string noEmoji)
		{
			if (!Global.ContainsUnicodeCharacter(yesEmoji))
			{
				await Context.Channel.SendMessageAsync("Your yes emoji is not a unicode!");
				return;
			}

			if (!Global.ContainsUnicodeCharacter(noEmoji))
			{
				await Context.Channel.SendMessageAsync("Your no emoji is not a unicode!");
				return;
			}

			await VotingService.StartVote(title, description, time, yesEmoji, noEmoji, Context.Guild, Context.Channel,
				Context.User);
		}

		//TODO: Add `votes` and `vote stop` command, move vote commands into their own class

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