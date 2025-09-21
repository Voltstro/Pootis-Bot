using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pootis_Bot.Shared;

namespace Pootis_Bot.Services.Server;

public class ServerSetupService
{
    private readonly IDbContextFactory<PootisBotDbContext> dbContextFactory;

    public ServerSetupService(IDbContextFactory<PootisBotDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }
    
    public async Task RemoveMessage(ulong guildId, Guid messageId)
    {
        await using PootisBotDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        
        await dbContext.ServerMessages.Where(x => x.Server.DiscordId == guildId && x.Id == messageId).ExecuteDeleteAsync();
    }
}