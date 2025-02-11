using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Modules;

[Group("", "Commands for profile related things")]
public class ProfileModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly PootisBotDbContext dbContext;
    
    public ProfileModule(PootisBotDbContext dbContext)
    {
        this.dbContext = dbContext;
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

        Profile profileProfile = dbContext.GetOrCreateUser(user);

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
    public async Task SetUserProfileMessage(string message)
    {
        //TODO: We should probs filter this message
        if (string.IsNullOrWhiteSpace(message))
        {
            await RespondAsync("Your message cannot just be empty or white space!");
            return;
        }

        Profile profileProfile = dbContext.GetOrCreateUser(Context.User);
        profileProfile.ProfileMessage = message;
        await dbContext.SaveChangesAsync();

        await RespondAsync("Your profile message was updated.");
    }
}