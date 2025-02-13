using System;
using System.Linq;
using Discord.WebSocket;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Helper;

public static class DbContextExtensions
{
    /// <summary>
    ///     Gets or creates a <see cref="Profile"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static Profile GetOrCreateUser(this PootisBotDbContext context, SocketUser user)
    {
        Profile? foundUser = context.Profiles.FirstOrDefault(x => x.DiscordId == user.Id);
        if (foundUser == null)
        {
            foundUser = new Profile
            {
                Id = Guid.NewGuid(),
                DiscordId = user.Id,
                Xp = 0
            };
            context.Profiles.Add(foundUser);
            context.SaveChanges();
        }
        
        return foundUser;
    }

    public static Server GetOrCreateServer(this PootisBotDbContext context, SocketGuild guild)
    {
        return GetOrCreateServer(context, guild.Id);
    }
    
    public static Server GetOrCreateServer(this PootisBotDbContext context, ulong guildId)
    {
        Server? foundServer = context.Servers.FirstOrDefault(x => x.DiscordId == guildId);
        if (foundServer == null)
        {
            foundServer = new Server
            {
                Id = Guid.NewGuid(),
                DiscordId = guildId,
            };
            
            context.Servers.Add(foundServer);
            context.SaveChanges();
        }
        
        return foundServer;
    }
}