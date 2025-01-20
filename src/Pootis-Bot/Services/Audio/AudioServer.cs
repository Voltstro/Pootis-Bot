using Discord;
using Discord.WebSocket;

namespace Pootis_Bot.Services.Audio;

public class AudioServer
{
    public AudioServer(ISocketMessageChannel textChannel, IVoiceChannel voiceChannel, bool isPlaying)
    {
        this.TextChannel = textChannel;
        this.VoiceChannel = voiceChannel;
        this.IsPlaying = isPlaying;
    }

    public ISocketMessageChannel TextChannel { get; init; }
    public IVoiceChannel VoiceChannel { get; init; }
    public bool IsPlaying { get; set; }
}