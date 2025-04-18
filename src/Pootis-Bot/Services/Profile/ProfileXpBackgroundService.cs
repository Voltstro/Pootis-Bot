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
using Pootis_Bot.Services.Core.Client;
using Pootis_Bot.Shared;

namespace Pootis_Bot.Services.Profile;

/// <summary>
///     Service for handling profile XP
/// </summary>
public class ProfileXpBackgroundService : IHostedService
{
    private readonly ILogger<ProfileXpBackgroundService> logger;
    private readonly IDbContextFactory<PootisBotDbContext> dbContextFactory;
    private readonly PootisBotConfig config;
    private readonly DiscordSocketClient client;
    
    public ProfileXpBackgroundService(
        ILogger<ProfileXpBackgroundService> logger,
        IDbContextFactory<PootisBotDbContext> dbContextFactory,
        IOptions<PootisBotConfig> config,
        ClientService clientService)
    {
        this.logger = logger;
        this.dbContextFactory = dbContextFactory;
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

        await using PootisBotDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        Shared.Models.Profile profile = dbContext.GetOrCreateUser(message.Author);
            
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
}