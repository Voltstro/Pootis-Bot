using System;
using Newtonsoft.Json;

namespace Pootis_Bot.Module.Profiles;

/// <summary>
///     Represents a profile for a Discord user
/// </summary>
public class Profile
{
    internal Profile(ulong id)
    {
        Id = id;
        Xp = 0;
        UserProfileMessage = "";
    }

    [JsonConstructor]
    internal Profile(ulong id, uint xp, string message)
    {
        Id = id;
        Xp = xp;
        UserProfileMessage = message;
    }

    public ulong Id { get; set; }

    public uint Xp { get; set; }

    public string UserProfileMessage { get; set; }

    /// <summary>
    ///     What level is the profile on?
    /// </summary>
    [JsonIgnore]
    public uint LevelNumber => (uint) Math.Sqrt(Xp / 30f);
}