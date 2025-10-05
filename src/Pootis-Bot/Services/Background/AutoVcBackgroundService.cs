using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Services.Core.Client;
using Pootis_Bot.Services.Server;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Services.Background;

/// <summary>
///     Background service for handling and monitoring AutoVcs
/// </summary>
public sealed class AutoVcBackgroundService : IHostedService
{
    private readonly ILogger<AutoVcBackgroundService> logger;
    private readonly DiscordSocketClient client;
    private readonly IServiceScopeFactory scopeFactory;
    
    private List<ActiveAutoVcChannel> activeAutoVcChannels;
    
    public AutoVcBackgroundService(ILogger<AutoVcBackgroundService> logger, ClientService clientService, IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        client = clientService.DiscordClient;
        this.scopeFactory = scopeFactory;
        
        activeAutoVcChannels = new List<ActiveAutoVcChannel>();
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        client.ChannelDestroyed += OnChannelDestroyed;
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        client.UserVoiceStateUpdated -= OnUserVoiceStateUpdated;
        client.ChannelDestroyed -= OnChannelDestroyed;
        
        return Task.CompletedTask;
    }

    private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState channelBefore, SocketVoiceState channelAfter)
    {
        using IServiceScope serviceScope = scopeFactory.CreateScope();
        
        AutoVcService autoVcService = serviceScope.ServiceProvider.GetRequiredService<AutoVcService>();
        ServerService serverService = serviceScope.ServiceProvider.GetRequiredService<ServerService>();
        
        //User connected to a channel
        SocketVoiceChannel? connectedChannel = channelAfter.VoiceChannel;
        if (connectedChannel != null)
        {
            logger.LogInformation("User {UserId} connected to channel {ChannelId}", user.Id, connectedChannel.Id);
            
            SocketGuild guild = channelAfter.VoiceChannel.Guild;
            Shared.Models.Server server = serverService.GetOrCreateServer(guild.Id);
            
            //User joined an autoVc channel
            AutoVC? autoVc = autoVcService.GetAutoVc(server.Id, connectedChannel.Id);
            if(autoVc == null)
                return;
            
            //Create new channel, copy options from base AutoVC channel
            int totalActiveAutoVcs = activeAutoVcChannels.Count(x => x.GuildId == guild.Id);

            //Make sure auto is maxed
            if (totalActiveAutoVcs > autoVc.MaxChannels)
            {
                logger.LogWarning("User {UserId} joined AutoVC {AtoVc} that was maxed.", user.Id, autoVc.Id);
                await guild.GetUser(user.Id).ModifyAsync(x => x.ChannelId = null);
                return;
            }
            
            //Create new channel
            RestVoiceChannel newChannel = await guild.CreateVoiceChannelAsync(
                $"{autoVc.BaseName} #{totalActiveAutoVcs + 1}",
                properties =>
                {
                    properties.CategoryId = connectedChannel.CategoryId;
                    properties.Bitrate = connectedChannel.Bitrate;
                    properties.Position = connectedChannel.Position + 1;
                    properties.UserLimit = autoVc.MaxUsers;
                });
            if (newChannel.CategoryId != null)
                await newChannel.SyncPermissionsAsync();

            activeAutoVcChannels.Add(new ActiveAutoVcChannel(guild.Id, newChannel.Id, autoVc.Id));
            
            //Move user to new channel
            await guild.GetUser(user.Id).ModifyAsync(x => x.ChannelId = newChannel.Id);
            
            //Modify autoVc channel it self to have less max users
            await connectedChannel.ModifyAsync(x =>
            {
                if (connectedChannel.UserLimit == null)
                    x.UserLimit = autoVc.MaxUsers;
                else
                    x.UserLimit = connectedChannel.UserLimit - 1;
            });
        }
        
        //User left a channel 
        SocketVoiceChannel? leftChannel = channelBefore.VoiceChannel;
        if (leftChannel != null)
        {
            logger.LogInformation("User {UserId} left channel {ChannelId}", user.Id, leftChannel.Id);
            
            SocketGuild guild = channelBefore.VoiceChannel.Guild;
            ActiveAutoVcChannel? activeAutoVcChannel = activeAutoVcChannels.FirstOrDefault(x => x.GuildId == guild.Id && x.ChannelId == leftChannel.Id);
            if(activeAutoVcChannel == null)
                return;
            
            AutoVC? autoVc = autoVcService.GetAutoVc(activeAutoVcChannel.ParentAutoVcId);
            if (autoVc == null)
            {
                logger.LogWarning("AutoVc {AutoVcId} seems to no longer exist!", activeAutoVcChannel.ParentAutoVcId);
                return;
            }

            
            if (leftChannel.ConnectedUsers.Count > 0)
                return;
            
            await leftChannel.DeleteAsync();
            activeAutoVcChannels.Remove(activeAutoVcChannel);
            
            //Increase auto again
            SocketVoiceChannel autoVcChannel = guild.GetVoiceChannel(autoVc.BaseVcChannelId);
            await autoVcChannel.ModifyAsync(x =>
            {
                if (autoVcChannel.UserLimit == null)
                    x.UserLimit = autoVc.MaxUsers;
                else
                    x.UserLimit = autoVcChannel.UserLimit + 1;
            });
        }
    }
    
    private async Task OnChannelDestroyed(SocketChannel channel)
    {
        if (channel.ChannelType != ChannelType.Voice)
            return;
        
        using IServiceScope serviceScope = scopeFactory.CreateScope();
        
        AutoVcService autoVcService = serviceScope.ServiceProvider.GetRequiredService<AutoVcService>();
        ServerService serverService = serviceScope.ServiceProvider.GetRequiredService<ServerService>();

        SocketGuild guild = ((SocketGuildChannel)channel).Guild;
        
        Shared.Models.Server server = serverService.GetOrCreateServer(guild.Id);
        
        AutoVC? autoVc = autoVcService.GetAutoVc(server.Id, channel.Id);
        if(autoVc == null)
            return;
        
        logger.LogInformation("AutoVc {AutoVcId} channel {ChannelId} has been deleted", autoVc.Id, channel.Id);
        
        //Delete any active sub channels
        ActiveAutoVcChannel[] activeAutoVcs = activeAutoVcChannels
            .Where(x => x.GuildId == guild.Id && x.ChannelId == channel.Id)
            .ToArray();

        foreach (ActiveAutoVcChannel activeAutoVc in activeAutoVcs)
        {
            SocketVoiceChannel? activeAutoVcChannel = guild.GetVoiceChannel(activeAutoVc.ChannelId);
            if(activeAutoVcChannel != null)
                await activeAutoVcChannel.DeleteAsync();
        }
        
        //Delete the record
        autoVcService.DeleteAutoVc(autoVc.Id);
    }

    private record ActiveAutoVcChannel(ulong GuildId, ulong ChannelId, Guid ParentAutoVcId);
}