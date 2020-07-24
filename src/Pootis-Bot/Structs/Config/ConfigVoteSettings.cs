using System;

namespace Pootis_Bot.Structs.Config
{
	/// <summary>
	/// Settings relating to voting
	/// </summary>
	public struct VoteSettings
	{
		/// <summary>
		/// Whats the max time a vote can run for
		/// </summary>
		public TimeSpan MaxVoteTime { get; set; }

		/// <summary>
		/// How many votes can be running at one time per guild
		/// </summary>
		public int MaxRunningVotesPerGuild { get; set; }
	}
}