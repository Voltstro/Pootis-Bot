using System;
using System.Linq;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Services;

/// <summary>
///     Backing service for management of servers
/// </summary>
public class ServerService
{
    private readonly PootisBotDbContext dbContext;
    
    public ServerService(PootisBotDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <summary>
    ///     Gets or create a server
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    public Server GetOrCreateServer(ulong guildId)
    {
        Server? foundServer = dbContext.Servers.FirstOrDefault(x => x.DiscordId == guildId);
        if (foundServer == null)
        {
            foundServer = new Server
            {
                Id = Guid.NewGuid(),
                DiscordId = guildId,
            };
            
            dbContext.Servers.Add(foundServer);
            dbContext.SaveChanges();
        }
        
        return foundServer;
    }

    /// <summary>
    ///     Gets a <see cref="Server"/> via its rule reaction's channel ID, message ID and emoji
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="messageId"></param>
    /// <param name="emoji"></param>
    /// <returns></returns>
    public Server? GetServerByRuleReactionChannelMessageAndEmoji(ulong channelId, ulong messageId, string emoji)
    {
        return dbContext.Servers.FirstOrDefault(x =>
            x.RuleReactionChannelId == channelId && x.RuleReactionMessageId == messageId &&
            x.RuleReactionEmoji == emoji);
    }

    /// <summary>
    ///     Updates a <see cref="Server"/>
    /// </summary>
    /// <param name="server"></param>
    public void UpdateServer(Server server)
    {
        dbContext.Servers.Update(server);
        dbContext.SaveChanges();
    }
}