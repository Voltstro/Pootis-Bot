namespace Pootis_Bot.Core.UserAccounts
{
    public class UserAccount
    {
        public ulong ID { get; set; }

        public uint Points { get; set; }

        public uint XP { get; set; }

        public uint NumberOfWarnings { get; set; }

        public bool IsNotWarnable { get; set; }
    }
}
