using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Core;

namespace Pootis_Bot.Module.Profiles;

[Group("", "Commands for profile related things")]
public class ProfileInteractions : InteractionModuleBase<SocketInteractionContext>
{
    private readonly SortProfilesComparer profilesComparer;
    private readonly ProfilesConfig profilesConfig;
    private readonly XpLevelManager levelManager;

    private string displayName;

    public ProfileInteractions(DiscordSocketClient client, XpLevelManager xpLevelManager)
    {
        profilesConfig = Config<ProfilesConfig>.Instance;
        BotConfig config = Config<BotConfig>.Instance;
        profilesComparer = new SortProfilesComparer();
        displayName = config.BotName;
        config.Saved += () => displayName = config.BotName;

        levelManager = xpLevelManager;
        client.MessageReceived += OnMessageReceived;
    }

    [SlashCommand("profile", "Gets a user's profile", true)]
    public async Task GetUserProfile(SocketUser? user = null)
    {
        user ??= Context.User;

        if (user.IsBot || user.IsWebhook)
        {
            await RespondAsync("Selected user is a bot or a webhook!");
            return;
        }

        Profile profile = profilesConfig.GetOrCreateProfile(user);

        EmbedBuilder embed = new();
        embed.WithTitle($"{user.Username}'s Profile");
        embed.WithFooter(profile.UserProfileMessage, user.GetAvatarUrl());
        embed.WithCurrentTimestamp();
        embed.WithThumbnailUrl(user.GetAvatarUrl(ImageFormat.Auto, 256));
        embed.AddField("Stats", $"**Level**: {profile.LevelNumber}\n**Xp**: {profile.Xp}\n", true);
        embed.AddField("Account",
            $"**Id**: {user.Id}\n**Creation Date**: {user.CreatedAt.DateTime.ToUniversalTime():yyyy MMMM dd h:mm tt UTC}");

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("profile-message", "Sets your user profile message")]
    public async Task SetUserProfileMessage(string message)
    {
        //TODO: We should probs filter this message
        if (string.IsNullOrWhiteSpace(message))
        {
            await RespondAsync("Your message cannot just be empty or white space!");
            return;
        }

        Profile profile = profilesConfig.GetOrCreateProfile(Context.User);
        profile.UserProfileMessage = message;
        profilesConfig.Save();

        await RespondAsync("Your profile message was updated.");
    }

    [SlashCommand("top10", "Gets the top10 profiles")]
    public async Task GetTop10Profiles()
    {
        Profile[] allProfiles = profilesConfig.GetAllProfiles();
        Array.Sort(allProfiles, profilesComparer);

        Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();
        sb.Append(
            $"```csharp\n 📋 Top 10 {displayName} Profiles for {Context.Guild.Name}\n =====================================\n");
        int count = 1;
        foreach (Profile user in allProfiles)
        {
            if (count > 10)
                break;

            SocketUser targetUser = Context.Guild.GetUser(user.Id);

            if (targetUser == null)
                continue;

            sb.Append(
                $"\n [{count}] -- # {targetUser.Username}\n         └ Level: {user.LevelNumber}\n         └ Xp: {user.Xp}");
            count++;
        }

        Profile callersProfile = profilesConfig.GetOrCreateProfile(Context.User);
        sb.Append(
            $"\n------------------------\n 😊 {Context.User.Username}'s Position: {Array.IndexOf(allProfiles, callersProfile) + 1}      {Context.User.Username}'s Level: {callersProfile.LevelNumber}      {Context.User.Username}'s Xp: {callersProfile.Xp}```");

        await RespondAsync(sb.ToString());
        sb.Dispose();
    }
    
    private async Task OnMessageReceived(SocketMessage message)
    {
        if (message is SocketUserMessage socketMessage)
            await levelManager.HandelUserMessage(socketMessage);
    }

    private class SortProfilesComparer : IComparer<Profile>
    {
        public int Compare(Profile? x, Profile? y)
        {
            if (y != null && x != null && x.Xp < y.Xp)
                return 1;
            if (y != null && x != null && x.Xp > y.Xp)
                return -1;
            return 0;
        }
    }
}