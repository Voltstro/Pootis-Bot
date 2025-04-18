using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Models.Audio;
using Pootis_Bot.Services.Audio;

namespace Pootis_Bot.Modules;

public class AudioModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<AudioModule> logger;
    private readonly AudioService audioService;
    private readonly AudioSelectionService audioSelectionService;
    
    public AudioModule(
        ILogger<AudioModule> logger,
        AudioService audioService, 
        AudioSelectionService audioSelectionService)
    {
        this.logger = logger;
        this.audioService = audioService;
        this.audioSelectionService = audioSelectionService;
    }

    [SlashCommand("join", "Join your current audio channel")]
    public async Task JoinChannel()
    {
        //Check user is connected to a voice channel
        IVoiceState? voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null)
        {
            await RespondAsync("You must be connected to a voice channel!");
            return;
        }

        //Check if bot has already connected to a voice channel
        if (audioService.TryGetAudioServer(Context.Guild, out AudioServer _))
        {
            await RespondAsync("I have already joined a voice channel!");
            return;
        }
        
        await audioService.JoinChannel(voiceState.VoiceChannel, Context.Channel);
        await RespondAsync("I have joined your voice channel.");
    }
    
    [SlashCommand("leave", "Leave your current audio channel")]
    public async Task LeaveChannel()
    {
        IVoiceState? voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null)
        {
            await RespondAsync("You must be connected to a voice channel!");
            return;
        }
        
        //Check if bot has connected to a voice channel
        if (!audioService.TryGetAudioServer(Context.Guild, out AudioServer audioServer))
        {
            await RespondAsync("I am already not connected to any voice channel!");
            return;
        }

        //Ensure user is in same voice channel
        if (audioServer.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be in the same voice channel as me before telling me to leave!");
            return;
        }

        await audioService.LeaveChannel(voiceState.VoiceChannel);
        await RespondAsync("I have left your voice channel.");
    }

    [SlashCommand("play", "Plays a song")]
    public async Task Play([Summary(description: "Search query to search for")] string? searchQuery = "")
    {
        IVoiceState? voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null)
        {
            await RespondAsync("You must be connected to a voice channel!");
            return;
        }
        
        //Check if bot has connected to a voice channel
        if (!audioService.TryGetAudioServer(Context.Guild, out AudioServer audioServer))
        {
            await RespondAsync("I am not connected to any voice channel!");
            return;
        }

        //Ensure user is in same voice channel
        if (audioServer.VoiceChannel.Id != voiceState.VoiceChannel.Id)
        {
            await RespondAsync("You need to be in the same voice channel as me before requesting to play audio!");
            return;
        }
        
        SocketGuild guild = Context.Guild;
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            bool paused = await audioService.IsPaused(guild);
            if (!paused)
            {
                await RespondAsync("A search query is required!");
                return;
            }

            await audioService.Resume(guild);
            await RespondAsync("Resumed playing current track.");
        }

        await RespondAsync("Searching...");
        IUserMessage responseAsync = await GetOriginalResponseAsync();

        AudioSearchResult result = await audioService.Search(searchQuery);
        if (!result.Successful)
        {
            await responseAsync.ModifyAsync(x =>
            {
                x.Content = "Sorry, but an error occured while searching. Please try again later.";
            });
            return;
        }

        if (result.AudioTracks == null)
        {
            await responseAsync.ModifyAsync(x =>
            {
                x.Content = "No search results where found using that query. Please try a different search query.";
            });
            return;
        }

        if (result.AudioTracks.Length == 1)
        {
            await audioService.Play(result.AudioTracks[0], Context.Guild);
            return;
        }

        //Get selections
        try
        {
            MessageComponent components =
                audioSelectionService.BuildSelectionMenu(result.AudioTracks);

            await responseAsync.ModifyAsync(x =>
            {
                x.Content = $"Multiple results were found, please select an option to add to the queue.";
                x.Components = components;
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating audio selection!");
        }
    }

    [SlashCommand("pause", "Pauses the current playing track")]
    public async Task Pause()
    {
        await audioService.Pause(Context.Guild);
        await RespondAsync("Current playing track has been paused. Use /play to continue playing.");
    }
}