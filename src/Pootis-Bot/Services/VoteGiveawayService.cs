using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;

namespace Pootis_Bot.Services
{
	public class VoteGiveawayService
	{
		public static readonly List<Vote> votes = new List<Vote>();
		public static bool IsVoteRunning;

		/// <summary>
		/// Starts a vote in a text channel
		/// </summary>
		/// <param name="guild">The guild the channel is in</param>
		/// <param name="channel">The channel to start the vote in</param>
		/// <param name="user">What user requested the vote?</param>
		/// <param name="time">How long should the vote go for?</param>
		/// <param name="title">The title of the vote</param>
		/// <param name="description">A description about what you're voting for</param>
		/// <param name="yesEmoji">An emoji for 'yes'</param>
		/// <param name="noEmoji">An emoji for 'no'</param>
		/// <returns></returns>
		public async Task StartVote(SocketGuild guild, ISocketMessageChannel channel, SocketUser user, string time,
			string title, string description, string yesEmoji, string noEmoji)
		{
			string[] times = time.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
			int totalTime = 0;

			// ReSharper disable once InconsistentNaming
			foreach (string _time in times)
			{
				TimeType timeType;

				string formatted;
				double currentTime;

				//Remove either h, hrs, hours, s, sec, secs, seconds, d, days depending on the time format
				if (_time.EndsWith("h") || time.EndsWith("hrs") || time.EndsWith("hours"))
				{
					formatted = _time.Replace("h", "").Replace("hrs", "").Replace("hours", "");
					timeType = TimeType.Hours;
				}
				else if (_time.EndsWith("s") || time.EndsWith("sec") || time.EndsWith("secs") ||
				         time.EndsWith("seconds"))
				{
					formatted = _time.Replace("s", "").Replace("sec", "").Replace("secs", "").Replace("seconds", "");
					timeType = TimeType.Secs;
				}
				else if (_time.EndsWith("d") || time.EndsWith("days"))
				{
					formatted = _time.Replace("d", "").Replace("days", "");
					timeType = TimeType.Days;
				}
				else //It didn't include a support 'format' i geuss u would call it?
				{
					await channel.SendMessageAsync(
						"Invaild time format, the time must end with either `h` for hours, `s` for seconds and `d` for days!");
					return;
				}

				if (!int.TryParse(formatted, out int temp)) //Convert the formated string to a int
				{
					await channel.SendMessageAsync(
						"The time foramt is incorrect! Make sure it is something like this: `1h`.");
					return;
				}

				//Convert the time depending on what TimeType it is.
				if (timeType == TimeType.Days)
				{
					TimeSpan tp = TimeSpan.FromDays(temp);
					currentTime = tp.TotalMilliseconds;
				}
				else if (timeType == TimeType.Hours)
				{
					TimeSpan tp = TimeSpan.FromHours(temp);
					currentTime = tp.TotalMilliseconds;
				}
				else
				{
					TimeSpan tp = TimeSpan.FromSeconds(temp);
					currentTime = tp.TotalMilliseconds;
				}

				totalTime += (int) currentTime;
			}

			//Setup emojis
			if (!Global.ContainsUnicodeCharacter(yesEmoji) || !Global.ContainsUnicodeCharacter(noEmoji))
			{
				await channel.SendMessageAsync(
					"Your emoji(s) are not unicode! Use https://unicode.org/emoji/charts/full-emoji-list.html to select an emoji.");
				return;
			}

			Emoji yesEmote = new Emoji(yesEmoji);
			Emoji noEmote = new Emoji(noEmoji);

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("**Vote**: " + title);
			embed.WithDescription(description +
			                      $"\n\nReact with {yesEmoji} to say **YES** or {noEmoji} to say **NO**.");
			embed.WithFooter($"Vote started by {user.Username} @ ", user.GetAvatarUrl());
			embed.WithCurrentTimestamp();

			RestUserMessage message = await channel.SendMessageAsync("", false, embed.Build());

			// Add yes and no reactions
			await message.AddReactionAsync(yesEmote);
			await message.AddReactionAsync(noEmote);

			IsVoteRunning = true;
			Vote vote = new Vote
			{
				VoteMessageId = message.Id,
				YesEmoji = yesEmoji,
				NoEmoji = noEmoji
			};

			votes.Add(vote);

			Logger.Log($"A vote has started on the guild {guild.Name}({guild.Id})");

			await Task.Delay(totalTime); // Wait for the vote to finish

			EmbedBuilder finishedVote = new EmbedBuilder();
			finishedVote.WithTitle("**Vote Results**: " + title);
			finishedVote.WithDescription(
				$"The vote has finished! Here are the results: \n**Yes**: {vote.YesCount}\n**No**: {vote.NoCount}");
			finishedVote.WithFooter($"Vote was started by {user.Username} at ", user.GetAvatarUrl());
			finishedVote.WithCurrentTimestamp();

			await channel.SendMessageAsync("", false, finishedVote.Build());

			votes.Remove(vote);

			Logger.Log($"The vote on {guild.Name}({guild.Id}) has finished.");

			//Check to make sure no other votes are running.
			if (votes.Count == 0)
				IsVoteRunning = false;
		}

		/// <summary>
		/// Gets a vote from it's id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Vote GetVote(ulong id)
		{
			IEnumerable<Vote> result = from a in votes
				where a.VoteMessageId == id
				select a;

			Vote server = result.FirstOrDefault();
			return server;
		}

		private enum TimeType
		{
			Days,
			Hours,
			Secs
		}

		public class Vote
		{
			public ulong VoteMessageId { get; set; }

			public string YesEmoji { get; set; }
			public string NoEmoji { get; set; }

			public int YesCount { get; set; }
			public int NoCount { get; set; }
		}
	}
}