using System;
using System.Threading;

namespace Pootis_Bot.Module.Upgrade.OldEntities.Server
{
    /// <summary>
    /// A <see cref="Vote"/>, you know? Were people can choose between options, like what a vote IS!
    /// </summary>
    public class Vote
    {
        /// <summary>
        /// The message ID to react onto
        /// </summary>
        public ulong VoteMessageId { get; set; }

        /// <summary>
        /// The channel were the vote is happening
        /// </summary>
        public ulong VoteMessageChannelId { get; set; }

        /// <summary>
        /// The title of the vote
        /// </summary>
        public string VoteTitle { get; set; }

        /// <summary>
        /// The description of a vote
        /// </summary>
        public string VoteDescription { get; set; }

        /// <summary>
        /// The ID of the user who started the vote
        /// </summary>
        public ulong VoteStarterUserId { get; set; }

        /// <summary>
        /// The <see cref="DateTime"/> of when the vote started
        /// </summary>
        public DateTime VoteStartTime { get; set; }

        /// <summary>
        /// How long will the vote last for?
        /// </summary>
        public TimeSpan VoteLastTime { get; set; }

        /// <summary>
        /// The emoji used to say 'yes'
        /// </summary>
        public string YesEmoji { get; set; }

        /// <summary>
        /// The emoji used to say 'no'
        /// </summary>
        public string NoEmoji { get; set; }

        /// <summary>
        /// The amount of people who have said 'yes'
        /// </summary>
        public int YesCount { get; set; }

        /// <summary>
        /// The amount of people who have said 'no'
        /// </summary>
        public int NoCount { get; set; }

        /// <summary>
        /// The cancellation token, to cancel or end a vote
        /// </summary>
        public CancellationTokenSource CancellationToken { get; set; }
    }
}