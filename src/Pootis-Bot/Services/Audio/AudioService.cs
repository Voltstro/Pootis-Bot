using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Models.Audio;
using Victoria;
using Victoria.Enums;
using Victoria.Rest.Search;
using Victoria.WebSocket.EventArgs;

namespace Pootis_Bot.Services.Audio;

public class AudioService
{
    private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode;
    private readonly DiscordSocketClient client;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<AudioService> logger;
    
    private readonly ConcurrentDictionary<ulong, AudioServer> guildAudioChannels;
    
    public AudioService(
        LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode,
        DiscordSocketClient client,
        IServiceProvider serviceProvider,
        ILogger<AudioService> logger)
    {
        this.lavaNode = lavaNode;
        this.client = client;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        
        lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;
        
        lavaNode.OnTrackEnd += OnTrackEndAsync;
        lavaNode.OnTrackStart += OnTrackStartAsync;
        lavaNode.OnTrackException += OnTrackException;
        
        client.Ready += ClientOnReady;
        client.UserVoiceStateUpdated += OnClientUserVoiceStateUpdated;

        guildAudioChannels = new ConcurrentDictionary<ulong, AudioServer>();
    }

    /// <summary>
    ///     Gets an audio server
    /// </summary>
    /// <param name="guild"></param>
    /// <param name="audioServer"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public bool TryGetAudioServer(IGuild guild, out AudioServer audioServer)
    {
        return guildAudioChannels.TryGetValue(guild.Id, out audioServer);
    }

    /// <summary>
    ///     Removes an audio server
    /// </summary>
    /// <param name="guild"></param>
    public void RemoveAudioServer(IGuild guild)
    {
        guildAudioChannels.Remove(guild.Id, out _);
    }
    
    /// <summary>
    ///     Checks if the bot has joined a channel in a guid
    /// </summary>
    /// <param name="guild"></param>
    /// <returns></returns>
    public async Task<bool> HasJoinedAChannel(IGuild guild)
    {
        LavaPlayer<LavaTrack>? player = await lavaNode.TryGetPlayerAsync(guild.Id);
        return player != null;
    }

    /// <summary>
    ///     Joins a voice channel
    /// </summary>
    /// <param name="voiceChannel"></param>
    /// <param name="messageChannel"></param>
    public async Task JoinChannel(IVoiceChannel voiceChannel, ISocketMessageChannel messageChannel)
    {
        await lavaNode.JoinAsync(voiceChannel);
        guildAudioChannels.TryAdd(voiceChannel.Guild.Id, new AudioServer(messageChannel, voiceChannel, false));
    }

    /// <summary>
    ///     Leaves a voice channel
    /// </summary>
    /// <param name="voiceChannel"></param>
    public async Task LeaveChannel(IVoiceChannel voiceChannel)
    {
        await lavaNode.LeaveAsync(voiceChannel);
        guildAudioChannels.TryRemove(voiceChannel.Guild.Id, out AudioServer _);
    }

    /// <summary>
    ///     Searches for audio
    /// </summary>
    /// <param name="searchQuery"></param>
    /// <param name="audioSource"></param>
    /// <returns></returns>
    public async Task<AudioSearchResult> Search(string searchQuery, AudioSource audioSource = AudioSource.YouTube)
    {
        string searchWithIdentifier = $"{audioSource.SourceToPrefixIdentifier()}:{searchQuery}";
        SearchResponse result = await lavaNode.LoadTrackAsync(searchWithIdentifier);
        
        //Error occured while searching
        if (result.Type == SearchType.Error)
        {
            logger.LogError("An error occured while searching with LavaLink! {Message}", result.Exception.Message);
            return new AudioSearchResult()
            {
                Successful = false
            };
        }
        
        //No results were found while searching
        if (result.Type == SearchType.Empty)
        {
            return new AudioSearchResult
            {
                Successful = true,
            };
        }
        
        return new AudioSearchResult
        {
            Successful = true,
            AudioTracks = result.Tracks.ToArray()
        };
    }

    /// <summary>
    ///     Starts playing a track
    /// </summary>
    /// <param name="track"></param>
    /// <param name="guild"></param>
    /// <exception cref="NullReferenceException"></exception>
    public async Task Play(LavaTrack track, IGuild guild)
    {
        logger.LogDebug("Track {TrackName} has been requested to be added to the queue.", track.Title);
        
        LavaPlayer<LavaTrack> player = await GetPlayer(guild);
        LavaQueue<LavaTrack> queue = player.GetQueue();
        TryGetAudioServer(guild, out AudioServer audioServer);
        
        //Play song now if there are no items in the queue
        if (!audioServer.IsPlaying && queue.Count == 0)
        {
            await player.PlayAsync(lavaNode, track);
            return;
        }
        
        queue.Enqueue(track);
    }
    
    /// <summary>
    ///     Check if track is paused
    /// </summary>
    /// <param name="guild"></param>
    /// <returns></returns>
    public async Task<bool> IsPaused(IGuild guild)
    {
        LavaPlayer<LavaTrack> player = await GetPlayer(guild);
        return player.IsPaused;
    }

    /// <summary>
    ///     Pauses playing
    /// </summary>
    /// <param name="guild"></param>
    /// <exception cref="NullReferenceException"></exception>
    public async Task Pause(IGuild guild)
    {
        LavaPlayer<LavaTrack> player = await GetPlayer(guild);
        await player.PauseAsync(lavaNode);
    }

    /// <summary>
    ///     Resumes playing
    /// </summary>
    /// <param name="guild"></param>
    /// <exception cref="NullReferenceException"></exception>
    public async Task Resume(IGuild guild)
    {
        LavaPlayer<LavaTrack> player = await GetPlayer(guild);
        await player.ResumeAsync(lavaNode, player.Track);
    }

    [DebuggerStepThrough]
    private async Task<LavaPlayer<LavaTrack>> GetPlayer(IGuild guild)
    {
        LavaPlayer<LavaTrack> player = await lavaNode.TryGetPlayerAsync(guild.Id);
        if (player == null)
            throw new NullReferenceException("Need to be connected to a VC");
        
        return player;
    }

    private async Task ClientOnReady()
    {
        await serviceProvider.UseLavaNodeAsync();
    }
    
    private async Task OnTrackStartAsync(TrackStartEventArg arg)
    {
        LavaTrack track = arg.Track;
        logger.LogDebug("Track {TrackName} has started.", track.Title);
        if (!guildAudioChannels.TryGetValue(arg.GuildId, out AudioServer guildInfo))
            throw new NullReferenceException("Failed getting guild info!");

        //Mark as playing and send a message saying so
        guildInfo.IsPlaying = true;
        await guildInfo.TextChannel.SendMessageAsync($"Now playing **{track.Title}** by **{track.Author}**.");
    }
        
    private async Task OnTrackEndAsync(TrackEndEventArg arg)
    {
        logger.LogDebug("Track {TrackName} has finished. Reason: {Reason}", arg.Track.Title, arg.Reason);

        if (!guildAudioChannels.TryGetValue(arg.GuildId, out AudioServer guildInfo))
            throw new NullReferenceException("Failed getting guild info!");

        guildInfo.IsPlaying = false;
        if(arg.Reason is TrackEndReason.Replaced or TrackEndReason.Stopped or TrackEndReason.Load_Failed or TrackEndReason.Cleanup)
            return;

        string message = $"**{arg.Track.Title}** has finished playing. No more songs to play, to add more, use /play.";
        LavaPlayer<LavaTrack> player = await lavaNode.TryGetPlayerAsync(arg.GuildId);
        LavaQueue<LavaTrack> queue = player.GetQueue();
        if (queue.TryDequeue(out LavaTrack nextTrack))
        {
            message = $"**{arg.Track.Title}** has finished playing.";
            await player.PlayAsync(lavaNode, nextTrack);
        }
        
        await SendAndLogMessageAsync(arg.GuildId, message);
    }
    
    private async Task OnTrackException(TrackExceptionEventArg arg)
    {
        logger.LogError("Error occured while playing {TrackName} on {GuildId}! Reason: {Reason}", arg.Track.Title, arg.GuildId, arg.Exception.Message);
        await SendAndLogMessageAsync(arg.GuildId, "Sorry, but an error occured while playing that track.");
    }
        
    private Task OnWebSocketClosedAsync(WebSocketClosedEventArg arg)
    {
        logger.LogError("Connection to LavaLink was closed! Reason: {Reason} Code: {Code}", arg.Reason, arg.Code);
        return Task.CompletedTask;
    }
        
    private async Task SendAndLogMessageAsync(ulong guildId, string message)
    {
        if (!guildAudioChannels.TryGetValue(guildId, out AudioServer guildInfo))
        {
            logger.LogError("Failed to find stored text channel for guild {GuildId}!", guildId);
            return;
        }

        await guildInfo.TextChannel.SendMessageAsync(message);
    }
    
    private async Task OnClientUserVoiceStateUpdated(SocketUser user, SocketVoiceState beforeVoiceState, SocketVoiceState afterVoiceState)
    {
        try
        {
            SocketVoiceChannel? voiceChannel = beforeVoiceState.VoiceChannel;
            if (voiceChannel == null)
                return;

            int totalConnectedUsers = voiceChannel.ConnectedUsers.Count;
            if (totalConnectedUsers > 1)
                return;
            
            SocketGuild guild = voiceChannel.Guild;
            if (!TryGetAudioServer(guild, out AudioServer audioServer))
                return;

            //Bot is connected to a voice channel were only the bot is connected to it
            if (audioServer.VoiceChannel.Id == voiceChannel.Id)
            {
                RemoveAudioServer(guild);
                await LeaveChannel(audioServer.VoiceChannel);
                await audioServer.TextChannel.SendMessageAsync(
                    "I have left the voice channel because there was no one else there!");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling client user voice state updated!");
        }
    }
}