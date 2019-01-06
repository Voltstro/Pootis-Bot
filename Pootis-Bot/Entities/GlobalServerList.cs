namespace Pootis_Bot.Entities
{
    public class GlobalServerList
    {
        public ulong serverID;

        public bool enableWelcome;

        public ulong welcomeID;

        public bool isRules;

        public string staffRoleName;

        public string adminRoleName;

        //Misc. command permissions
        public string permEmbedMessage;

        //Profile mang. command permissions
        public string permNotWarnableRole;
        public string permMakeWarnableRole;
        public string permWarn;
        public string permYT;
        public string permGiphy;
        public string permGoogle;
        
    }
}
