using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Core;
using Victoria;

namespace Pootis_Bot.Services.Audio;

public sealed class AudioSelectionService
{
    private const string AudioSelectionId = "AudioSelect";

    private readonly ILogger<AudioSelectionService> logger;
    private readonly DiscordSocketClient client;
    private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode;
    private readonly AudioService audioService;

    private int lastLavaTrackId;
    private readonly Dictionary<int, string> lavaTracks;
    
    public AudioSelectionService(
        ILogger<AudioSelectionService> logger,
        DiscordSocketClient client,
        LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode,
        AudioService audioService)
    {
        this.logger = logger;
        this.client = client;
        this.lavaNode = lavaNode;
        this.audioService = audioService;
        
        client.SelectMenuExecuted += ClientOnSelectMenuExecuted;
        
        lavaTracks = new Dictionary<int, string>();
    }

    public MessageComponent BuildSelectionMenu(LavaTrack[] tracks, IGuild guild, IUserMessage userMessage)
    {
        SelectMenuBuilder menu = new()
        {
            CustomId = AudioSelectionId,
            Placeholder = "Select what song to play.",
            MaxValues = 1,
            MinValues = 1,
        };
        
        for (int i = 0; i < Math.Clamp(tracks.Length, 0, 4); i++)
        {
            LavaTrack track = tracks[i];
            string title = Utils.Truncate(track.Title, 53);
            string author = Utils.Truncate(track.Author, 15);

            menu.AddOption($"{title} by {author}", GetIdForTrack(track).ToString());
        }
        
        ComponentBuilder builder = new ComponentBuilder()
            .WithSelectMenu(menu);

        return builder.Build();
    }

    private int GetIdForTrack(LavaTrack lavaTrack)
    {
        string hash = lavaTrack.Hash;
        KeyValuePair<int, string> track = lavaTracks.FirstOrDefault(x => x.Value == hash);
        if (track.Value != null)
            return track.Key;
        
        //New track
        int id = lastLavaTrackId++;
        lavaTracks.Add(id, hash);
        return id;
    }

    private async Task<LavaTrack> GetTrackFromId(int id)
    {
        if (!lavaTracks.TryGetValue(id, out string trackHash))
            throw new NullReferenceException();

        return await lavaNode.DecodeTrackAsync(trackHash);
    }
    
    private async Task ClientOnSelectMenuExecuted(SocketMessageComponent messageComponent)
    {
        try
        {
            if(messageComponent.Data.CustomId != AudioSelectionId)
                return;

            string trackId = messageComponent.Data.Values.First();

            LavaTrack track = await GetTrackFromId(int.Parse(trackId));
            await messageComponent.RespondAsync(
                $"**{track.Title}** by **{track.Author}** has been added to the queue.");

            //TODO: Update selection to be disabled
            
            logger.LogDebug("Handled on select menu executed. Got track {TrackName}.", track.Title);
            SocketGuild? guild = client.GetGuild(messageComponent.GuildId!.Value);

            await audioService.Play(track, guild);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when handling client button execute!");
        }
    }
    
    private class AudioSelectionData
    {
        public AudioSelectionData(Guid id, IGuild guild, LavaTrack[] tracks, IUserMessage message)
        {
            Id = id;
            Guild = guild;
            LavaTracks = tracks;
            UserMessage = message;
        }
        
        public Guid Id { get; }
        
        public IGuild Guild { get; }
        
        public LavaTrack[] LavaTracks { get; }
        
        public IUserMessage UserMessage { get; }
    }
}
