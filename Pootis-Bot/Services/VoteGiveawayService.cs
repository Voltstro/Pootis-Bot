using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services
{
    public class VoteGiveawayService
    {
        private enum TimeType { Days, Hours, Secs}

        public static List<Vote> votes = new List<Vote>();
        public static bool isVoteRunning;

        public async Task StartVote(SocketGuild guild, ISocketMessageChannel channel, SocketUser user, string time, string title, string description, string yesEmoji, string noEmoji)
        {
            string[] times = time.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            int totalTime = 0;

            foreach(var _time in times)
            {
                TimeType timeType;

                string formated;
                double currentTime = 0;

                //Remove either h, hrs, hours, s, sec, secs, seconds, d, days depending on the time format
                if (_time.EndsWith("h") || time.EndsWith("hrs") || time.EndsWith("hours"))
                {
                    formated = _time.Replace("h", "").Replace("hrs", "").Replace("hours", "");
                    timeType = TimeType.Hours;
                }                  
                else if (_time.EndsWith("s") || time.EndsWith("sec") || time.EndsWith("secs") || time.EndsWith("seconds"))
                {
                    formated = _time.Replace("s", "").Replace("sec", "").Replace("secs", "").Replace("seconds", "");
                    timeType = TimeType.Secs;
                }   
                else if (_time.EndsWith("d") || time.EndsWith("days"))
                {
                    formated = _time.Replace("d", "").Replace("days", "");
                    timeType = TimeType.Days;
                }  
                else //It didn't include a support 'format' i geuss u would call it?
                {
                    await channel.SendMessageAsync("Invaild time format, the time must end with either `h` for hours, `s` for seconds and `d` for days!");
                    return;
                }

                if (!int.TryParse(formated, out int temp)) //Convert the formated string to a int
                {
                    await channel.SendMessageAsync("The time foramt is incorrect! Make sure it is something like this: `1h`.");
                    return;
                }

                //Convert the time depending on what TimeType it is.
                if (timeType == TimeType.Days)
                {
                    TimeSpan tp = TimeSpan.FromDays(temp);
                    currentTime = tp.TotalMilliseconds;
                }
                else if(timeType == TimeType.Hours)
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
            if(!Global.ContainsUnicodeCharacter(yesEmoji) || !Global.ContainsUnicodeCharacter(noEmoji))
            {
                await channel.SendMessageAsync("Your emoji(s) are not unicode! Use https://unicode.org/emoji/charts/full-emoji-list.html to select an emoji.");
                return;
            }

            var yesEmote = new Emoji(yesEmoji);
            var noEmote = new Emoji(noEmoji);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("**Vote**: " + title);
            embed.WithDescription(description + $"\n\nReact with {yesEmoji} to say **YES** or {noEmoji} to say **NO**.");
            embed.WithFooter($"Vote started by {user.Username} @ ", user.GetAvatarUrl());
            embed.WithCurrentTimestamp();

            var message = await channel.SendMessageAsync("", false, embed.Build());

            // Add yes and no reactions
            await message.AddReactionAsync(yesEmote);
            await message.AddReactionAsync(noEmote);

            isVoteRunning = true;
            var vote = new Vote
            {
                VoteMessageID = message.Id,
                YesEmoji = yesEmoji,
                NoEmoji = noEmoji
            };

            votes.Add(vote);

            Global.Log($"A vote has started on the guild {guild.Name}({guild.Id})", ConsoleColor.Green);

            await Task.Delay(totalTime); // Wait for the vote to finish

            EmbedBuilder finishedVote = new EmbedBuilder();
            finishedVote.WithTitle("**Vote Results**: " + title);
            finishedVote.WithDescription($"The vote has finished! Here are the results: \n**Yes**: {vote.YesCount}\n**No**: {vote.NoCount}");
            finishedVote.WithFooter($"Vote was started by {user.Username} at ", user.GetAvatarUrl());
            finishedVote.WithCurrentTimestamp();

            await channel.SendMessageAsync("", false, finishedVote.Build());

            votes.Remove(vote);

            Global.Log($"The vote on {guild.Name}({guild.Id}) has finished.", ConsoleColor.Green);

            //Check to make sure no other votes are running.
            if (votes.Count == 0)
                isVoteRunning = false;
        }

        public static Vote GetVote(ulong id)
        {
            var result = from a in votes
                         where a.VoteMessageID == id
                         select a;

            var server = result.FirstOrDefault();
            return server;
        }

        public class Vote
        {
            public ulong VoteMessageID { get; set; }
            
            public string YesEmoji { get; set; }
            public string NoEmoji { get; set; }

            public int YesCount { get; set; }
            public int NoCount { get; set; }
        }

    }
}
