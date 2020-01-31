using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Services.Voting;

namespace Pootis_Bot.Modules.Basic
{
	public class VotingCommands : ModuleBase<SocketCommandContext>
	{
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

		[Command("vote end", RunMode = RunMode.Async)]
		[Summary("Ends a vote")]
		public async Task EndVote([Remainder] ulong voteId = 0)
		{
			//Cancel last vote the user started
			if (voteId == 0)
			{
				UserAccount user = UserAccountsManager.GetAccount((SocketGuildUser)Context.User);
				if (user.UserLastVoteId != 0)
					await VotingService.EndVote(
						ServerListsManager.GetServer(Context.Guild).GetVote(user.UserLastVoteId), Context.Guild);
				else
					await Context.Channel.SendMessageAsync(
						"You don't appear to have any votes running, if you do, put in the ID of the vote message with this command as well!");
			}
			else
			{
				Vote vote = ServerListsManager.GetServer(Context.Guild).GetVote(voteId);
				if (vote == null)
				{
					await Context.Channel.SendMessageAsync("There are no votes with that ID!");
					return;
				}

				if (((SocketGuildUser) Context.User).GuildPermissions.ManageMessages)
				{
					await VotingService.EndVote(vote, Context.Guild);
					await Context.Channel.SendMessageAsync("That vote was ended.");
				}
				else if (vote.VoteStarterUserId != Context.User.Id)
				{
					await VotingService.EndVote(vote, Context.Guild);
					await Context.Channel.SendMessageAsync("Your vote was ended.");
				}
			}
		}
	}
}
