using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Services;

/// <summary>
///     Backing service for AutoVCs
/// </summary>
public class AutoVcService
{
    private readonly PootisBotDbContext dbContext;
    
    public AutoVcService(PootisBotDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <summary>
    ///     Creates a new AutoVC
    /// </summary>
    /// <param name="serverId"></param>
    /// <param name="channelId"></param>
    /// <param name="name"></param>
    /// <param name="maxChannels"></param>
    /// <param name="maxUsers"></param>
    /// <returns></returns>
    public AutoVC CreateAutoVc(Guid serverId, ulong channelId, string name, int maxChannels = 3, int maxUsers = 25)
    {
        AutoVC autoVC = new()
        {
            Id = Guid.NewGuid(),
            ServerId = serverId,
            BaseVcChannelId = channelId,
            BaseName = name,
            MaxChannels = maxChannels,
            MaxUsers = maxUsers
        };
        
        dbContext.AutoVCs.Add(autoVC);
        dbContext.SaveChanges();

        return autoVC;
    }

    /// <summary>
    ///     Gets an AutoVc via <see cref="Server.Id"/> and channelId
    /// </summary>
    /// <param name="serverId"></param>
    /// <param name="channelId"></param>
    /// <returns></returns>
    public AutoVC? GetAutoVc(Guid serverId, ulong channelId)
    {
        return dbContext.AutoVCs
            .AsNoTracking()
            .FirstOrDefault(x => x.ServerId == serverId && x.BaseVcChannelId == channelId);
    }

    /// <summary>
    ///     Gets an AutoVa via <see cref="AutoVC.Id"/>
    /// </summary>
    /// <param name="autoVcId"></param>
    /// <returns></returns>
    public AutoVC? GetAutoVc(Guid autoVcId)
    {
        return dbContext.AutoVCs
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == autoVcId);
    }

    /// <summary>
    ///     Deletes an Auto via <see cref="AutoVC.Id"/>
    /// </summary>
    /// <param name="autoVcId"></param>
    public void DeleteAutoVc(Guid autoVcId)
    {
        dbContext.AutoVCs.Where(x => x.Id == autoVcId).ExecuteDelete();
        dbContext.SaveChanges();
    }
}