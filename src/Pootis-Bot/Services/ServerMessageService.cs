using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pootis_Bot.Helper;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Messages;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Services;

/// <summary>
///     Service for <see cref="ServerMessage"/>
/// </summary>
public sealed class ServerMessageService
{
    private readonly PootisBotDbContext dbContext;

    public ServerMessageService(PootisBotDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <summary>
    ///     Adds a <see cref="ServerMessage"/>
    /// </summary>
    /// <param name="server"></param>
    /// <param name="messageType"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<ServerMessage> AddMessage(Pootis_Bot.Shared.Models.Server server, MessageType messageType, string message)
    {
        ServerMessage serverMessage = new()
        {
            ServerId = server.Id,
            Type = messageType,
            Message = message
        };
        await dbContext.ServerMessages.AddAsync(serverMessage);
        await dbContext.SaveChangesAsync();
        return serverMessage;
    }

    /// <summary>
    ///     Gets all <see cref="ServerMessage"/> of <see cref="MessageType"/>
    /// </summary>
    /// <param name="server"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public async Task<ServerMessage[]> GetAllMessagesOfType(Pootis_Bot.Shared.Models.Server server,
        MessageType messageType)
    {
        return await dbContext.ServerMessages
            .Where(x => x.ServerId == server.Id && x.Type == messageType)
            .ToArrayAsync();
    }

    /// <summary>
    ///     Gets if any message of <see cref="MessageType"/> is available
    /// </summary>
    /// <param name="server"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public async Task<bool> GetIsAnyMessageOfType(Pootis_Bot.Shared.Models.Server server, MessageType messageType)
    {
        return await dbContext.ServerMessages.AnyAsync(x => x.ServerId == server.Id && x.Type == messageType);
    }
    
    /// <summary>
    ///     Deletes a <see cref="ServerMessage"/>
    /// </summary>
    /// <param name="server"></param>
    /// <param name="messageId"></param>
    public async Task DeleteMessage(Pootis_Bot.Shared.Models.Server server, Guid messageId)
    {
        await dbContext.ServerMessages
            .Where(x => x.Server.DiscordId == server.DiscordId && x.Id == messageId)
            .ExecuteDeleteAsync();
    }
}