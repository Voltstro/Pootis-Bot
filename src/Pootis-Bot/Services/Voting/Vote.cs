using System;

namespace Pootis_Bot.Services.Voting
{
	public class Vote
	{
		public ulong VoteMessageId { get; set; }
		
		public ulong VoteMessageChannelId { get; set; }

		public string VoteTitle { get; set; }
		public string VoteDescription { get; set; }

		public ulong VoteStarterUserId { get; set; }

		public DateTime VoteStartTime { get; set; }
		public TimeSpan VoteLastTime { get; set; }

		public string YesEmoji { get; set; }
		public string NoEmoji { get; set; }

		public int YesCount { get; set; }
		public int NoCount { get; set; }
	}
}