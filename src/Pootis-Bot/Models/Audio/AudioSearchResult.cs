using Victoria;

namespace Pootis_Bot.Models.Audio;

public class AudioSearchResult
{
    /// <summary>
    ///     Was the search successful?
    /// </summary>
    public bool Successful { get; init; }
    
    /// <summary>
    ///     Found audio tracks
    /// </summary>
    public LavaTrack[]? AudioTracks { get; init; }
}