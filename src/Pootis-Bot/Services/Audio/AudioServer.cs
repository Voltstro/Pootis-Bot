using Discord;
using Discord.WebSocket;

namespace Pootis_Bot.Services.Audio;

public class AudioServer
{
    public AudioServer(ISocketMessageChannel textChannel, IVoiceChannel voiceChannel, bool isPlaying)
    {
        TextChannel = textChannel;
        VoiceChannel = voiceChannel;
        IsPlaying = isPlaying;
    }

    public ISocketMessageChannel TextChannel { get; }
    public IVoiceChannel VoiceChannel { get; }
    public bool IsPlaying { get; set; }
}