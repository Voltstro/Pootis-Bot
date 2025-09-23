using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pootis_Bot.Helper;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Messages;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Services.Server;

public class ServerSetupService
{
    private readonly IDbContextFactory<PootisBotDbContext> dbContextFactory;

    public ServerSetupService(IDbContextFactory<PootisBotDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task AddMessage(ulong guildId, MessageType messageType, string message)
    {
        await using PootisBotDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        Shared.Models.Server server = dbContext.GetOrCreateServer(guildId);

        ServerMessage serverMessage = new()
        {
            ServerId = server.Id,
            Type = messageType,
            Message = message
        };
        await dbContext.ServerMessages.AddAsync(serverMessage);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task RemoveMessage(ulong guildId, Guid messageId)
    {
        await using PootisBotDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        
        await dbContext.ServerMessages.Where(x => x.Server.DiscordId == guildId && x.Id == messageId).ExecuteDeleteAsync();
    }
}