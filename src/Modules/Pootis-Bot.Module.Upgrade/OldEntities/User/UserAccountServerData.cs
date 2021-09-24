namespace Pootis_Bot.Module.Upgrade.OldEntities.User
{
    /// <summary>
    /// User data that is per server
    /// </summary>
    public class UserAccountServerData
    {
        /// <summary>
        /// What is the ID of the server
        /// </summary>
        public ulong ServerId { get; set; }

        /// <summary>
        /// How many warnings does a user have on this server
        /// </summary>
        public int Warnings { get; set; }

        /// <summary>
        /// Is the account NOT warnable (if true the account cannot be warned)
        /// </summary>
        public bool IsAccountNotWarnable { get; set; }

        /// <summary>
        /// Is the user muted?
        /// </summary>
        public bool IsMuted { get; set; }

        /// <summary>
        /// How many points does the user have
        /// </summary>
        public uint Points { get; set; }
    }
}