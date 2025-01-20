using System;

namespace Pootis_Bot.Services.Audio;

/// <summary>
///     Utils for Audio
/// </summary>
public static class AudioUtils
{
    public static string SourceToPrefixIdentifier(this AudioSource audioSource)
    {
        switch (audioSource)
        {
            case AudioSource.YouTube:
                return "ytsearch";
            default:
                throw new ArgumentOutOfRangeException(nameof(audioSource), audioSource, null);
        }
    }
}