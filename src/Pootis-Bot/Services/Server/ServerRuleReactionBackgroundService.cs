using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Helper;
using Pootis_Bot.Shared;

namespace Pootis_Bot.Services.Server;

/// <summary>
///     Background service for handling server rule reactions
/// </summary>
public class ServerRuleReactionBackgroundService : IHostedService
{
    private readonly ILogger<ServerRuleReactionBackgroundService> logger;
    private readonly IDbContextFactory<PootisBotDbContext> dbContextFactory;
    private readonly DiscordSocketClient client;
    
    public ServerRuleReactionBackgroundService(ILogger<ServerRuleReactionBackgroundService> logger, IDbContextFactory<PootisBotDbContext> dbContextFactory, DiscordSocketClient client)
    {
        this.logger = logger;
        this.dbContextFactory = dbContextFactory;
        this.client = client;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.ReactionAdded += ClientOnReactionAdded;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        client.ReactionAdded -= ClientOnReactionAdded;
        return Task.CompletedTask;
    }
    
    private async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> userMessage, Cacheable<IMessageChannel, ulong> messageChannel, SocketReaction reaction)
    {
        await using PootisBotDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        ulong channelId = messageChannel.Id;
        ulong messageId = userMessage.Id;
        string emoji = reaction.Emote.Name;
        ulong userId = reaction.UserId;
        
        logger.LogDebug("Got reaction on {ChannelId}/{MessageId} for {Emoji} by {UserId}", channelId, messageId, emoji, userId);

        Shared.Models.Server? server = dbContext.Servers.FirstOrDefault(x => x.RuleReactionChannelId == channelId && x.RuleReactionMessageId == messageId && x.RuleReactionEmoji == emoji);
        if(server is not { RuleReactionEnabled: true } || server.RuleReactionRoleId == null)
            return;

        //Assign role (if user doesn't have it)
        ulong roleId = server.RuleReactionRoleId.Value;
        SocketGuild guild = client.GetGuild(server.DiscordId);
        SocketGuildUser user = guild.GetUser(userId);
        if(user.HasRole(roleId))
            return;
        
        await user.AddRoleAsync(roleId);
    }
}