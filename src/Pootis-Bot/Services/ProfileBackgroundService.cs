using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pootis_Bot.Core;
using Pootis_Bot.Helper;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Services;

/// <summary>
///     Service for handling profile interactions, such as XP
/// </summary>
public class ProfileBackgroundService : IHostedService
{
    private readonly ILogger<ProfileBackgroundService> logger;
    private readonly IDbContextFactory<PootisBotDbContext> dbContextFactory;
    private readonly PootisBotConfig config;
    private readonly DiscordSocketClient client;
    
    public ProfileBackgroundService(
        ILogger<ProfileBackgroundService> logger,
        IDbContextFactory<PootisBotDbContext> dbContextFactory,
        IOptions<PootisBotConfig> config,
        DiscordSocketClient client)
    {
        this.logger = logger;
        this.dbContextFactory = dbContextFactory;
        this.config = config.Value;
        this.client = client;
    }

    private async Task ClientOnMessageReceived(SocketMessage message)
    {
        SocketUser author = message.Author;
        if(author.IsBot || author.IsWebhook)
            return;

        await using PootisBotDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        Profile profile = dbContext.GetOrCreateUser(message.Author);
            
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
        await dbContext.SaveChangesAsync();
        
        //New level
        if (profile.LevelNumber > lastLevel)
            await message.Channel.SendMessageAsync($"{author.Mention} leveled up! Now on level **{profile.LevelNumber}**!");
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
}