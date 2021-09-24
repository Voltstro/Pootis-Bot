using System;

namespace Pootis_Bot.Module.Upgrade.OldEntities.Server
{
    /// <summary>
    /// Settings relating to voting
    /// </summary>
    internal struct VoteSettings
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