using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Module.Profiles;

internal class XpLevelManager
{
    private readonly ProfilesConfig profilesConfig;
    private readonly List<UserLevelData> users;

    internal XpLevelManager()
    {
        profilesConfig = Config<ProfilesConfig>.Instance;
        users = new List<UserLevelData>();
    }

    internal async Task HandelUserMessage(SocketUserMessage message)
    {
        SocketUser messageAuthor = message.Author;
        if(messageAuthor.IsBot || messageAuthor.IsWebhook)
            return;
        
        UserLevelData user = GetOrCreateUser(message);

        //Make sure the message isn't the same
        string userMessage = message.Content.ToLower();
        if (userMessage == user.LastMessage)
        {
            user.LastMessage = userMessage;
            return;
        }

        //Check cooldown time
        if ((DateTime.UtcNow - user.LastMessageThatAddedXpTime).TotalSeconds <=
            profilesConfig.XpGiveCooldown.TotalSeconds)
            return;

        //Add XP to the user
        Profile profile = profilesConfig.GetOrCreateProfile(messageAuthor);
        uint lastLevel = profile.LevelNumber;
        AddXpToUser(profile);
        user.LastMessage = userMessage;
        user.LastMessageThatAddedXpTime = DateTime.UtcNow;

        //We reached a new level
        if (profile.LevelNumber > lastLevel)
            await LevelUpMessage(message.Channel, messageAuthor, profile);
    }

    internal void AddXpToUser(Profile profile)
    {
        profile.Xp += profilesConfig.XpGiveAmount;
        profilesConfig.Save();
        Logger.Debug("Added {XpAmount} XP to user {UserId}", profilesConfig.XpGiveAmount, profile.Id);
    }

    private static async Task LevelUpMessage(ISocketMessageChannel channel, IMentionable user, Profile profile)
    {
        await channel.SendMessageAsync($"{user.Mention} leveled up! Now on level **{profile.LevelNumber}**!");
    }

    private UserLevelData GetOrCreateUser(SocketMessage message)
    {
        IEnumerable<UserLevelData> result = from a in users
            where a.UserId == message.Author.Id
            select a;

        UserLevelData user = result.FirstOrDefault() ?? CreateUser(message.Author.Id, message.Content.ToLower());
        return user;
    }

    private UserLevelData CreateUser(ulong id, string message)
    {
        UserLevelData user = new(id, DateTime.UtcNow, message);
        users.Add(user);
        return user;
    }

    private class UserLevelData
    {
        public string LastMessage;
        public DateTime LastMessageThatAddedXpTime;

        public readonly ulong UserId;

        public UserLevelData(ulong userId, DateTime lastMessageThatAddedXpTime, string lastMessage)
        {
            UserId = userId;
            LastMessageThatAddedXpTime = lastMessageThatAddedXpTime;
            LastMessage = lastMessage;
        }
    }
}