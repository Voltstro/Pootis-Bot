using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Services.Core.Client;
using Pootis_Bot.Shared.Messages;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Services.Background;

/// <summary>
///     Background service for handling server welcome and goodbye messages
/// </summary>
public sealed class ServerWelcomeGoodbyeBackgroundService : IHostedService
{
    private readonly ILogger<ServerWelcomeGoodbyeBackgroundService> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly DiscordSocketClient client;
    
    public ServerWelcomeGoodbyeBackgroundService(
        ILogger<ServerWelcomeGoodbyeBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        ClientService clientService)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
        client = clientService.DiscordClient;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.UserJoined += ClientOnUserJoined;
        client.UserLeft += ClientOnUserLeft;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        client.UserJoined -= ClientOnUserJoined;
        client.UserLeft -= ClientOnUserLeft;
        return Task.CompletedTask;
    }
    
    private async Task ClientOnUserJoined(SocketGuildUser user)
    {
        SocketGuild guild = user.Guild;
        logger.LogInformation("{User} joined server {Server}.", user.Username, guild.Name);

        if(user.IsBot || user.IsWebhook)
            return;
        
        using IServiceScope serviceScope = scopeFactory.CreateScope();
        
        ServerService serverService = serviceScope.ServiceProvider.GetRequiredService<ServerService>();
        Shared.Models.Server server = serverService.GetOrCreateServer(guild.Id);
        
        //Ensure welcome message is enable, and a channel is set
        if(!server.WelcomeMessageEnabled || server.WelcomeGoodbyeChannelId == null)
            return;

        //Get welcome/goodbye channel
        SocketTextChannel? channel = guild.GetTextChannel(server.WelcomeGoodbyeChannelId!.Value);
        if (channel == null)
        {
            logger.LogWarning("Server {ServerId} has a welcome/goodbye channel ID {ChannelId} set that does not exist!", server.Id, server.WelcomeGoodbyeChannelId);
            return;
        }
        
        await HandleMessage(MessageType.Welcome, guild, user, server, channel, serviceScope);
    }
    
    private async Task ClientOnUserLeft(SocketGuild guild, SocketUser user)
    {
        logger.LogInformation("{User} left server {Server}.", user.Username, guild.Name);
        if(user.IsBot || user.IsWebhook)
            return;
        
        using IServiceScope serviceScope = scopeFactory.CreateScope();
        
        ServerService serverService = serviceScope.ServiceProvider.GetRequiredService<ServerService>();
        Shared.Models.Server server = serverService.GetOrCreateServer(guild.Id);
        
        //Ensure goodbye message is enable, and a channel is set
        if(!server.GoodbyeMessageEnabled || server.WelcomeGoodbyeChannelId == null)
            return;

        //Get welcome/goodbye channel
        SocketTextChannel? channel = guild.GetTextChannel(server.WelcomeGoodbyeChannelId!.Value);
        if (channel == null)
        {
            logger.LogWarning("Server {ServerId} has a welcome/goodbye channel ID {ChannelId} set that does not exist!", server.Id, server.WelcomeGoodbyeChannelId);
            return;
        }

        await HandleMessage(MessageType.Goodbye, guild, user, server, channel, serviceScope);
    }
    
    private async Task HandleMessage(MessageType type, SocketGuild guild, SocketUser user, Shared.Models.Server server, SocketTextChannel textChannel, IServiceScope serviceScope)
    {
        ServerMessageService serverMessageService = serviceScope.ServiceProvider.GetRequiredService<ServerMessageService>();
        ServerMessage[] messages = await serverMessageService.GetAllMessagesOfType(server, type);
        
        //Randomly select one
        int index = Random.Shared.Next(0, messages.Length);
        ServerMessage serverMessage = messages[index];
        
        //Format message
        string message = serverMessage.Message.Replace("%SERVER%", guild.Name).Replace("%USER%", type == MessageType.Goodbye ? user.Username : user.Mention);
        await textChannel.SendMessageAsync(message);
    }
}