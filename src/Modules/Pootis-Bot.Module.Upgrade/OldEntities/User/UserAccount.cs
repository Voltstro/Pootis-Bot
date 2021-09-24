using System.Collections.Generic;
using System.ComponentModel;

namespace Pootis_Bot.Module.Upgrade.OldEntities.User
{
    /// <summary>
    /// A user account
    /// </summary>
    public class UserAccount
    {
        /// <summary>
        /// The ID of the user
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// How much XP does this user have? Xp/Level number are across servers and are NOT server specific
        /// </summary>
        public uint Xp { get; set; }

        /// <summary>
        /// What message does the user have set for their profile
        /// </summary>
        [DefaultValue("")]
        public string ProfileMsg { get; set; }

        /// <summary>
        /// The ID of the vote that was the user's last vote
        /// </summary>
        [DefaultValue(0)]
        public ulong UserLastVoteId { get; set; }

        /// <summary>
        /// A list of all the user's <see cref="UserAccountServerData"/>
        /// </summary>
        public List<UserAccountServerData> Servers { get; set; }
    }
}