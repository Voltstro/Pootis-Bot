using System;

namespace Pootis_Bot.Structs.Config
{
	public struct VoteSettings
	{
		public TimeSpan MaxVoteTime { get; set; }
		public int MaxRunningVotesPerGuild { get; set; }
	}
}
