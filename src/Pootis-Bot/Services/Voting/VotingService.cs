using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Services.Voting
{
	public class VotingService
	{
		/// <summary>
		/// Starts and adds a new vote to a server
		/// </summary>
		/// <param name="voteTitle"></param>
		/// <param name="voteDescription"></param>
		/// <param name="lastTime"></param>
		/// <param name="yesEmoji"></param>
		/// <param name="noEmoji"></param>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <param name="userWhoExecuted"></param>
		/// <returns></returns>
		public static async Task StartVote(string voteTitle, string voteDescription, TimeSpan lastTime, string yesEmoji,
			string noEmoji, SocketGuild guild, IMessageChannel channel, SocketUser userWhoExecuted)
		{
			//Setup Emojis
			Emoji yesEmote = new Emoji(yesEmoji);
			Emoji noEmote = new Emoji(noEmoji);

			//Setup embed
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("Setting up vote...");

			//Send the message and add the initial reactions
			IUserMessage voteMessage = await channel.SendMessageAsync("", false, embed.Build());
			await voteMessage.AddReactionAsync(yesEmote);
			await voteMessage.AddReactionAsync(noEmote);

			Vote newVote = new Vote
			{
				VoteMessageId = voteMessage.Id,
				VoteMessageChannelId = channel.Id,
				VoteTitle = voteTitle,
				VoteDescription = voteDescription,
				VoteStarterUserId = userWhoExecuted.Id,
				NoCount = 0,
				YesCount = 0,
				NoEmoji = noEmoji,
				YesEmoji = yesEmoji,
				VoteLastTime = lastTime,
				VoteStartTime = DateTime.Now,
				CancellationToken = new CancellationTokenSource()
			};

			//User last vote
			UserAccountsManager.GetAccount((SocketGuildUser) userWhoExecuted).UserLastVoteId = voteMessage.Id;
			UserAccountsManager.SaveAccounts();

			//Add our vote to the server list
			ServerListsManager.GetServer(guild).Votes.Add(newVote);
			ServerListsManager.SaveServerList();

			embed.WithTitle(voteTitle);
			embed.WithDescription(voteDescription +
			                      $"\nReact to this message with {yesEmoji} to say **YES** or react with {noEmoji} to say **NO**.");
			embed.WithFooter($"Vote started by: {userWhoExecuted}", userWhoExecuted.GetAvatarUrl());

			await MessageUtils.ModifyMessage(voteMessage, embed);

			await RunVote(newVote, guild);
		}

		/// <summary>
		/// Executes and waits for a vote to end
		/// </summary>
		/// <param name="vote"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static async Task RunVote(Vote vote, SocketGuild guild)
		{
			//Get the time difference
			TimeSpan timeDifference = DateTime.Now.Subtract(vote.VoteStartTime);
			TimeSpan timeTillRun = vote.VoteLastTime.Subtract(timeDifference);

			Logger.Log($"Started running a vote, will end in {timeTillRun.TotalMilliseconds} milliseconds.",
				LogVerbosity.Debug);

			//If the vote is is already less then 700 milliseconds till it ends, then just end it now
			if (timeTillRun.TotalMilliseconds < 700)
			{
				await EndVote(vote, guild);
				return;
			}

			//Do our task delay
			try
			{
				await Task.Delay(Convert.ToInt32(Math.Round(timeTillRun.TotalMilliseconds, 0)),
					vote.CancellationToken.Token);

				if (vote.CancellationToken.IsCancellationRequested)
					return;

				await EndVote(vote, guild);
			}
			catch (TaskCanceledException)
			{
				//Don't need to do anything
			}
			catch (Exception ex)
			{
#if DEBUG
				Logger.Log($"Some error occured while a vote was ended, here are the details: {ex}", LogVerbosity.Error);
#else
				Logger.Log($"Some error occured while a vote was ended, here is the message: {ex.Message}", LogVerbosity.Debug);
#endif
			}
		}

		/// <summary>
		/// Ends a vote running on a guild
		/// <para>If the vote is running, it will END it!</para>
		/// </summary>
		/// <param name="vote">The vote to end</param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static async Task EndVote(Vote vote, SocketGuild guild)
		{
			Logger.Log("The vote ended.", LogVerbosity.Debug);

			vote.CancellationToken.Cancel();

			SocketUser user = guild.GetUser(vote.VoteStarterUserId);

			//Remove from user's last vote
			UserAccountsManager.GetAccount((SocketGuildUser) user).UserLastVoteId = 0;
			UserAccountsManager.SaveAccounts();

			//Create a new embed with the results
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle(vote.VoteTitle);
			embed.WithDescription(vote.VoteDescription +
			                      $"\nThe vote is now over! Here are the results:\n**Yes**: {vote.YesCount}\n**No**: {vote.NoCount}");
			if (user != null)
				embed.WithFooter($"Vote started by: {user}", user.GetAvatarUrl());
			else
				embed.WithFooter("Vote started by: a person who left the guild :(");

			//Modify the message
			IMessage message =
				await guild.GetTextChannel(vote.VoteMessageChannelId).GetMessageAsync(vote.VoteMessageId);
			await MessageUtils.ModifyMessage(message as IUserMessage, embed);

			//Send the user who started the vote a message about their vote is over
			if (user != null)
			{
				EmbedBuilder userDmEmbed = new EmbedBuilder();
				userDmEmbed.WithTitle("Vote: " + vote.VoteTitle);
				userDmEmbed.WithDescription($"Your vote that you started on the **{guild.Name}** guild is now over.\n" +
				                            $"You can see the results [here](https://discordapp.com/channels/{guild.Id}/{vote.VoteMessageChannelId}/{vote.VoteMessageId}).");

				IDMChannel userDm = await user.GetOrCreateDMChannelAsync();
				await userDm.SendMessageAsync("", false, userDmEmbed.Build());
			}

			//Remove our vote from the server's vote list
			ServerListsManager.GetServer(guild).Votes.Remove(vote);
			ServerListsManager.SaveServerList();
		}
	}
}