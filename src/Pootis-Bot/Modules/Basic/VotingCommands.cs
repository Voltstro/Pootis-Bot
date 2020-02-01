using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services.Voting;
using Pootis_Bot.TypeReaders;

namespace Pootis_Bot.Modules.Basic
{
	public class VotingCommands : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Provides commands for votes
		// Contributors     - Creepysin, 

		[Command("vote", RunMode = RunMode.Async)]
		[Summary("Starts a vote")]
		public async Task Vote(string title, string description, string yesEmoji, string noEmoji, [Remainder] [OverrideTypeReader(typeof(TimeSpanCustomReader))] TimeSpan time)
		{
			if (!yesEmoji.ContainsUnicodeCharacter())
			{
				await Context.Channel.SendMessageAsync("Your yes emoji is not a unicode!");
				return;
			}

			if (!noEmoji.ContainsUnicodeCharacter())
			{
				await Context.Channel.SendMessageAsync("Your no emoji is not a unicode!");
				return;
			}

			await VotingService.StartVote(title, description, time, yesEmoji, noEmoji, Context.Guild, Context.Channel,
				Context.User);
		}

		[Command("vote end", RunMode = RunMode.Async)]
		[Summary("Ends a vote")]
		public async Task EndVote([Remainder] string voteId = "0")
		{
			//The input is just a number
			if(ulong.TryParse(voteId, NumberStyles.None, CultureInfo.InvariantCulture, out ulong id))
			{
				//Cancel last vote the user started
				if (id == 0)
				{
					UserAccount user = UserAccountsManager.GetAccount((SocketGuildUser)Context.User);
					if (user.UserLastVoteId != 0)
						await VotingService.EndVote(
							ServerListsManager.GetServer(Context.Guild).GetVote(user.UserLastVoteId), Context.Guild);
					else
						await Context.Channel.SendMessageAsync(
							"You don't appear to have any votes running, if you do, put in the ID of the vote message with this command as well!");
					return;
				}

				Vote vote = ServerListsManager.GetServer(Context.Guild).GetVote(id);
				if (vote == null)
				{
					await Context.Channel.SendMessageAsync("There are no votes with that ID!");
					return;
				}

				if (((SocketGuildUser) Context.User).GuildPermissions.ManageMessages)
				{
					await VotingService.EndVote(vote, Context.Guild);
					await Context.Channel.SendMessageAsync("That vote was ended.");
					return;
				}
				if (vote.VoteStarterUserId != Context.User.Id)
				{
					await VotingService.EndVote(vote, Context.Guild);
					await Context.Channel.SendMessageAsync("Your vote was ended.");
					return;
				}
			}

			//End all votes
			if (voteId.RemoveWhitespace() == "*")
			{
				if (!((SocketGuildUser) Context.User).GuildPermissions.ManageMessages)
				{
					await Context.Channel.SendMessageAsync("You don't have permissions to end all votes!");
					return;
				}

				Vote[] votes = ServerListsManager.GetServer(Context.Guild).Votes.ToArray();
				foreach (Vote vote in votes)
				{
					await VotingService.EndVote(vote, Context.Guild);
				}

				await Context.Channel.SendMessageAsync("All votes were ended.");
				return;
			}

			await Context.Channel.SendMessageAsync(
				"Unknown argument! Either needs to be none or a message ID to end a vote, or `*` to end all.");
		}
	}
}
