using System;
using System.Linq;
using Pootis_Bot.Shared;

namespace Pootis_Bot.Services.Server;

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
    public Shared.Models.Server GetOrCreateServer(ulong guildId)
    {
        Shared.Models.Server? foundServer = dbContext.Servers.FirstOrDefault(x => x.DiscordId == guildId);
        if (foundServer == null)
        {
            foundServer = new Shared.Models.Server
            {
                Id = Guid.NewGuid(),
                DiscordId = guildId,
            };
            
            dbContext.Servers.Add(foundServer);
            dbContext.SaveChanges();
        }
        
        return foundServer;
    }
}