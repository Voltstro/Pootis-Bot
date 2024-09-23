using System;
using System.Collections.Concurrent;
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

namespace Pootis_Bot.Services;

public class AudioService
{
    private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode;
    private readonly DiscordSocketClient client;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<AudioService> logger;
    
    private readonly ConcurrentDictionary<ulong, GuildInfo> textChannels;
    
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
        
        this.client.Ready += ClientOnReady;

        textChannels = new ConcurrentDictionary<ulong, GuildInfo>();
    }

    /// <summary>
    ///     Joins a voice channel
    /// </summary>
    /// <param name="voiceChannel"></param>
    /// <param name="messageChannel"></param>
    public async Task JoinChannel(IVoiceChannel voiceChannel, ISocketMessageChannel messageChannel)
    {
        await lavaNode.JoinAsync(voiceChannel);
        textChannels.TryAdd(voiceChannel.Guild.Id, new GuildInfo(messageChannel));
    }

    /// <summary>
    ///     Leaves a voice channel
    /// </summary>
    /// <param name="voiceChannel"></param>
    public async Task LeaveChannel(IVoiceChannel voiceChannel)
    {
        await lavaNode.LeaveAsync(voiceChannel);
        textChannels.TryRemove(voiceChannel.Guild.Id, out GuildInfo _);
    }

    /// <summary>
    ///     Is there currently a track
    /// </summary>
    /// <param name="guild"></param>
    /// <returns></returns>
    public async Task<bool> IsTrack(IGuild guild)
    {
        LavaPlayer<LavaTrack> player = await lavaNode.TryGetPlayerAsync(guild.Id);
        if (player == null)
            throw new NullReferenceException("Need to be connected to a VC");

        return player.Track != null;
    }

    public async Task<bool> IsPaused(IGuild guild)
    {
        LavaPlayer<LavaTrack> player = await lavaNode.TryGetPlayerAsync(guild.Id);
        if (player == null)
            throw new NullReferenceException("Need to be connected to a VC");

        return player.IsPaused;
    }

    public async Task<AudioSearchResult> Search(string searchQuery)
    {
        SearchResponse result = await lavaNode.LoadTrackAsync(searchQuery);
        
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
        
        //TODO: Provide multiple track options
        LavaTrack track = result.Tracks.FirstOrDefault()!;
        return new AudioSearchResult
        {
            Successful = true,
            AudioTracks = result.Tracks.ToArray()
        };
    }

    public async Task Play(LavaTrack track, IGuild guild)
    {
        LavaPlayer<LavaTrack> player = await lavaNode.TryGetPlayerAsync(guild.Id);
        if (player == null)
            throw new NullReferenceException("Need to be connected to a VC");

        LavaQueue<LavaTrack> queue = player.GetQueue();

        if (!textChannels.TryGetValue(guild.Id, out GuildInfo? guildInfo))
            throw new NullReferenceException("Failed getting guild info!");
        
        if (!guildInfo.IsPlaying && queue.Count == 0)
        {
            await player.PlayAsync(lavaNode, track);
            return;
        }
        
        queue.Enqueue(track);
    }

    public async Task Pause(IGuild guild)
    {
        LavaPlayer<LavaTrack> player = await lavaNode.TryGetPlayerAsync(guild.Id);
        if (player == null)
            throw new NullReferenceException("Need to be connected to a VC");
        
        await player.PauseAsync(lavaNode);
    }

    public async Task Resume(IGuild guild)
    {
        LavaPlayer<LavaTrack> player = await lavaNode.TryGetPlayerAsync(guild.Id);
        if (player == null)
            throw new NullReferenceException("Need to be connected to a VC");

        await player.ResumeAsync(lavaNode, player.Track);
    }

    private async Task ClientOnReady()
    {
        await serviceProvider.UseLavaNodeAsync();
    }
    
    private Task OnTrackStartAsync(TrackStartEventArg arg)
    {
        logger.LogDebug("Track {TrackName} has started.", arg.Track.Title);
       
        if (!textChannels.TryGetValue(arg.GuildId, out GuildInfo? guildInfo))
            throw new NullReferenceException("Failed getting guild info!");

        guildInfo.IsPlaying = true;

        return Task.CompletedTask;
    }
        
    private async Task OnTrackEndAsync(TrackEndEventArg arg)
    {
        logger.LogDebug("Track {TrackName} has finished. Reason: {Reason}", arg.Track.Title, arg.Reason);

        if (!textChannels.TryGetValue(arg.GuildId, out GuildInfo? guildInfo))
            throw new NullReferenceException("Failed getting guild info!");

        guildInfo.IsPlaying = false;

        if(arg.Reason != TrackEndReason.Finished)
            return;
        
        string message;
        LavaPlayer<LavaTrack> player = await lavaNode.TryGetPlayerAsync(arg.GuildId);
        LavaQueue<LavaTrack> queue = player.GetQueue();
        if (!queue.TryDequeue(out LavaTrack nextTrack))
        {
            message = $"**{arg.Track.Title}** has finished playing. No more songs to play, to add more, use /play.";
        }
        else
        {
            message = $"**{arg.Track.Title}** has finished playing. Now playing **{nextTrack.Title}** by **{nextTrack.Author}**.";
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
        if (!textChannels.TryGetValue(guildId, out GuildInfo? guildInfo))
        {
            logger.LogError("Failed to find stored text channel for guild {GuildId}!", guildId);
            return;
        }

        await guildInfo.TextChannel.SendMessageAsync(message);
    }

    private class GuildInfo
    {
        public GuildInfo(ISocketMessageChannel messageChannel)
        {
            TextChannel = messageChannel;
            IsPlaying = false;
        }
        
        public ISocketMessageChannel TextChannel { get; init; }

        public bool IsPlaying { get; set; }
    }
}