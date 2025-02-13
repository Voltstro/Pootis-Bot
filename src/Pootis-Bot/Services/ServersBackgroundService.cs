using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Helper;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Models;
using MessageType = Pootis_Bot.Shared.Messages.MessageType;

namespace Pootis_Bot.Services;

/// <summary>
///     Background service related to handling servers
/// </summary>
public class ServersBackgroundService : IHostedService
{
    private readonly ILogger<ServersBackgroundService> logger;
    private readonly IDbContextFactory<PootisBotDbContext> dbContextFactory;
    private readonly DiscordSocketClient client;
    
    public ServersBackgroundService(ILogger<ServersBackgroundService> logger, IDbContextFactory<PootisBotDbContext> dbContextFactory, DiscordSocketClient client)
    {
        this.logger = logger;
        this.dbContextFactory = dbContextFactory;
        this.client = client;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.UserJoined += ClientOnUserJoined;
        client.UserLeft += ClientOnUserLeft;
        
        client.ReactionAdded += ClientOnReactionAdded;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        client.UserJoined -= ClientOnUserJoined;
        client.UserLeft -= ClientOnUserLeft;
        
        client.ReactionAdded -= ClientOnReactionAdded;
        return Task.CompletedTask;
    }

    #region Welcome / Goodbye Messages
    
    private async Task ClientOnUserJoined(SocketGuildUser user)
    {
        await using PootisBotDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        SocketGuild guild = user.Guild;
        
        logger.LogInformation("{User} joined server {Server}.", user.Username, guild.Name);
        
        Server server = dbContext.GetOrCreateServer(guild);
        
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
        
        await HandleMessage(MessageType.Welcome, guild, user, server, channel, dbContext);
    }
    
    private async Task ClientOnUserLeft(SocketGuild guild, SocketUser user)
    {
        await using PootisBotDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        Server server = dbContext.GetOrCreateServer(guild);
        
        logger.LogInformation("{User} left server {Server}.", user.Username, guild.Name);
        
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

        await HandleMessage(MessageType.Goodbye, guild, user, server, channel, dbContext);
    }

    private async Task HandleMessage(MessageType type, SocketGuild guild, SocketUser user, Server server, SocketTextChannel textChannel, PootisBotDbContext dbContext)
    {
        //Get goodbye messages
        ServerMessage[] goodbyeMessages = await dbContext.ServerMessages
            .Where(x => x.ServerId == server.Id && x.Type == type)
            .ToArrayAsync();
        
        //Randomly select one
        int index = Random.Shared.Next(0, goodbyeMessages.Length);
        ServerMessage serverMessage = goodbyeMessages[index];
        
        //Format message
        string message = serverMessage.Message.Replace("%SERVER%", guild.Name).Replace("%USER%", type == MessageType.Goodbye ? user.Username : user.Mention);
        await textChannel.SendMessageAsync(message);
    }
    
    #endregion

    #region Rele Reactions

    private async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> userMessage, Cacheable<IMessageChannel, ulong> messageChannel, SocketReaction reaction)
    {
        await using PootisBotDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        ulong channelId = messageChannel.Id;
        ulong messageId = userMessage.Id;
        string emoji = reaction.Emote.Name;
        ulong userId = reaction.UserId;
        
        logger.LogDebug("Got reaction on {ChannelId}/{MessageId} for {Emoji} by {UserId}", channelId, messageId, emoji, userId);

        Server? server = dbContext.Servers.FirstOrDefault(x => x.RuleReactionChannelId == channelId && x.RuleReactionMessageId == messageId && x.RuleReactionEmoji == emoji);
        if(server is not { RuleReactionEnabled: true } || server.RuleReactionRoleId == null)
            return;

        //Assign role (if user doesn't have it)
        ulong roleId = server.RuleReactionRoleId.Value;
        SocketGuild guild = client.GetGuild(server.DiscordId);
        SocketGuildUser user = guild.GetUser(userId);
        if(user.HasRole(roleId))
            return;
        
        await user.AddRoleAsync(roleId);
    }

    #endregion
}