using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pootis_Bot.Core
{
    internal class LevelingSystem
    {
        internal static async void UserSentMessage(SocketGuildUser user, SocketTextChannel channel, uint amount)
        {
            var userAccount = UserAccounts.GetAccount(user);
            uint oldLevel = userAccount.LevelNumber;
            userAccount.XP += amount;
            UserAccounts.SaveAccounts();

            if (oldLevel != userAccount.LevelNumber)
            {
                await channel.SendMessageAsync(user.Mention + " leved up! Now on level on " + userAccount.LevelNumber);    
            }
        }
    }
}
