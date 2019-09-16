using Discord.WebSocket;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
    public static class LevelingSystem
    {
        public static async void UserSentMessage(SocketGuildUser user, SocketTextChannel channel, uint amount)
        {
            GlobalUserAccount userAccount = UserAccounts.GetAccount(user);
            uint oldLevel = userAccount.LevelNumber;
            userAccount.XP += amount;
            UserAccounts.SaveAccounts();

            if (oldLevel != userAccount.LevelNumber)
            {
                await channel.SendMessageAsync($"{user.Mention} leved up! Now on level **{userAccount.LevelNumber}**!");
            }
        }
    }
}
