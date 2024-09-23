using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Core;
using Pootis_Bot.Models.Audio;
using Victoria;

namespace Pootis_Bot.Services;

public sealed class AudioSelectionService
{
    private const string AudioSelectionId = "AudioSelect";

    private readonly ILogger<AudioSelectionService> logger;
    private readonly List<AudioSelectionData> selectionData;

    public delegate Task AudioSelectionMade(LavaTrack track, IGuild guild);
    
    /// <summary>
    ///     Invoked when a user makes an audio selection
    /// </summary>
    public event AudioSelectionMade OnSelectionMade; 
    
    public AudioSelectionService(ILogger<AudioSelectionService> logger, DiscordSocketClient client)
    {
        client.ButtonExecuted += ClientOnButtonExecute;

        this.logger = logger;
        selectionData = new List<AudioSelectionData>();
    }

    public MessageComponent BuildSelectionMenu(LavaTrack[] tracks, IGuild guild, IUserMessage userMessage)
    {
        ComponentBuilder builder = new();
        Guid id = Guid.NewGuid();
        for (int i = 0; i < Math.Clamp(tracks.Length, 0, 4); i++)
        {
            LavaTrack track = tracks[i];
            string title = Utils.Truncate(track.Title, 53);
            string author = Utils.Truncate(track.Author, 15);
            
            //builder.WithButton();

            builder.AddRow(new ActionRowBuilder().WithButton($"**{title}** by **{author}**",
                $"{AudioSelectionId}-{id:N}-{i}"));
        }
        
        
        
        selectionData.Add(new AudioSelectionData(id, guild, tracks, userMessage));
        return builder.Build();
    }
    
    private async Task ClientOnButtonExecute(SocketMessageComponent messageComponent)
    {
        try
        {
            string customId = messageComponent.Data.CustomId;
            if (!customId.StartsWith(AudioSelectionId))
                return;

            string[] splitResults = customId.Split('-');
            if (splitResults.Length != 3)
            {
                logger.LogWarning("Got button id {Id}, which did not split to expected size!", customId);
                return;
            }

            Guid id = Guid.Parse(splitResults[1]);
            AudioSelectionData? foundSelectionData = selectionData.FirstOrDefault(x => x.Id == id);
            if (foundSelectionData == null)
            {
                logger.LogWarning("Got id {Id}, which did not exist in stored selection data!", id);
                return;
            }

            int songId = int.Parse(splitResults[2]);
            LavaTrack selectedTrack = foundSelectionData.LavaTracks[songId];
            
            await OnSelectionMade.Invoke(selectedTrack, foundSelectionData.Guild);

            await messageComponent.RespondAsync(
                $"**{selectedTrack.Title}** by **{selectedTrack.Author}** has been added to the queue.");

            selectionData.Remove(foundSelectionData);
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
