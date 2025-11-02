using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pootis_Bot.Core;
using Pootis_Bot.Services.Core.Client;

namespace Pootis_Bot.Services.Background;

/// <summary>
///     Background service for handling profile XP
/// </summary>
public class ProfileXpBackgroundService : IHostedService
{
    private readonly ILogger<ProfileXpBackgroundService> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly PootisBotConfig config;
    private readonly DiscordSocketClient client;
    
    public ProfileXpBackgroundService(
        ILogger<ProfileXpBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<PootisBotConfig> config,
        ClientService clientService)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
        this.config = config.Value;
        client = clientService.DiscordClient;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.MessageReceived += ClientOnMessageReceived;
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        client.MessageReceived -= ClientOnMessageReceived;
        
        return Task.CompletedTask;
    }
    
    private async Task ClientOnMessageReceived(SocketMessage message)
    {
        SocketUser author = message.Author;
        if(author.IsBot || author.IsWebhook)
            return;

        using IServiceScope serviceScope = scopeFactory.CreateScope();
        ProfileService profileService = serviceScope.ServiceProvider.GetRequiredService<ProfileService>();
        
        Shared.Models.Profile profile = profileService.GetOrCreateProfile(message.Author.Id);
            
        //Check cooldown time
        if (profile.LastXpMessageTime != null)
        {
            if ((DateTime.UtcNow - profile.LastXpMessageTime.Value).TotalSeconds <=
                config.XpGiveCooldown.TotalSeconds)
                return;
        }
        
        //Add Xp
        uint lastLevel = profile.LevelNumber;
        profile.Xp += config.XpGiveAmount;
        profile.LastXpMessageTime = DateTime.UtcNow;
        
        logger.LogDebug("Added {XpAmount} XP to user {UserId}", config.XpGiveAmount, profile.Id);
        profileService.UpdateProfile(profile);
        
        //New level
        if (profile.LevelNumber > lastLevel)
            await message.Channel.SendMessageAsync($"{author.Mention} leveled up! Now on level **{profile.LevelNumber}**!");
    }
}