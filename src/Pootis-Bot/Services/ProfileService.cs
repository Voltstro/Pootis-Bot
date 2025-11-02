using System;
using System.Linq;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Services;

/// <summary>
///     Service for dealing with <see cref="Profile"/>
/// </summary>
public sealed class ProfileService
{
    private readonly PootisBotDbContext dbContext;
    
    public ProfileService(PootisBotDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <summary>
    ///     Gets or creates a <see cref="Profile"/>
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Profile GetOrCreateProfile(ulong userId)
    {
        Profile? foundUser = dbContext.Profiles.FirstOrDefault(x => x.DiscordId == userId);
        if (foundUser == null)
        {
            foundUser = new Profile
            {
                Id = Guid.NewGuid(),
                DiscordId = userId,
                Xp = 0
            };
            dbContext.Profiles.Add(foundUser);
            dbContext.SaveChanges();
        }
        
        return foundUser;
    }

    /// <summary>
    ///     Updates a <see cref="Profile"/>
    /// </summary>
    /// <param name="profile"></param>
    public void UpdateProfile(Profile profile)
    {
        dbContext.Profiles.Update(profile);
        dbContext.SaveChanges();
    }
}