using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Pootis_Bot.Services;
using Pootis_Bot.Shared.Helper;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Modules;

[Group("", "Commands for profile related things")]
public class ProfileModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ProfileService profileService;
    
    public ProfileModule(ProfileService profileService)
    {
        this.profileService = profileService;
    }
    
    [SlashCommand("profile", "Gets a user's profile", true)]
    public async Task GetUserProfile(SocketUser? user = null)
    {
        user ??= Context.User;

        if (user.IsBot || user.IsWebhook)
        {
            await RespondAsync(Messages.ValidationFailed(nameof(user), "not a bot or webhook"));
            return;
        }
        
        Profile profileProfile = profileService.GetOrCreateProfile(user.Id);
        
        EmbedBuilder embed = new();
        embed.WithTitle($"{user.Username}'s Profile");
        embed.WithFooter(profileProfile.ProfileMessage, user.GetAvatarUrl());
        embed.WithCurrentTimestamp();
        embed.WithThumbnailUrl(user.GetAvatarUrl(ImageFormat.Auto, 256));
        embed.AddField("Stats", $"**Level**: {profileProfile.LevelNumber}\n**Xp**: {profileProfile.Xp}\n", true);
        embed.AddField("Account",
            $"**Id**: {user.Id}\n**Creation Date**: {user.CreatedAt.DateTime.ToUniversalTime():yyyy MMMM dd h:mm tt UTC}");

        await RespondAsync(embed: embed.Build());
    }
    
    [SlashCommand("profile-message", "Sets your user profile message")]
    public async Task SetUserProfileMessage([MaxLength(25)] string message)
    {
        //TODO: We should probs filter this message
        if (string.IsNullOrWhiteSpace(message))
        {
            await RespondAsync(Messages.ValidationFailed(nameof(message), "not empty or contain only whitespace"));
            return;
        }

        Profile profile = profileService.GetOrCreateProfile(Context.User.Id);
        profile.ProfileMessage = message;
        profileService.UpdateProfile(profile);

        await RespondAsync(Messages.SetSuccessful("profile message", message));
    }
}