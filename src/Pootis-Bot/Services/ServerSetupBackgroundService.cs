using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Helper;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Services;

/// <summary>
///     Service related to server setup
///     Related to <see cref="Pootis_Bot.Modules.SetupModule"/>
/// </summary>
public sealed class ServerSetupBackgroundService : IHostedService
{
    public const string ServerSetupRuleReactionModalId = "ServerSetupRuleReactionModal";
    public const string ServerSetupRuleReactionModalMessageId = "ServerSetupRuleReactMessage";

    private readonly ILogger<ServerSetupBackgroundService> logger;
    private readonly IDbContextFactory<PootisBotDbContext> dbContextFactory;
    private readonly DiscordSocketClient client;
    
    public ServerSetupBackgroundService(ILogger<ServerSetupBackgroundService> logger,  IDbContextFactory<PootisBotDbContext> dbContextFactory, DiscordSocketClient client)
    {
        this.logger = logger;
        this.dbContextFactory = dbContextFactory;
        this.client = client;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.ModalSubmitted += ClientOnModalSubmitted;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        client.ModalSubmitted -= ClientOnModalSubmitted;
        return Task.CompletedTask;
    }
    
    private async Task ClientOnModalSubmitted(SocketModal modal)
    {
        //Not a modal we care about
        if (modal.Data.CustomId != ServerSetupRuleReactionModalId || modal.GuildId == null)
            return;

        //Get message input value
        SocketMessageComponentData messageInputData = modal.Data.Components.First(x => x.CustomId == ServerSetupRuleReactionModalMessageId);

        await using PootisBotDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        Server server = dbContext.GetOrCreateServer(modal.GuildId.Value);

        //Get saved channel
        SocketTextChannel? channel = client.GetGuild(server.DiscordId).GetTextChannel(server.RuleReactionChannelId!.Value);
        
        //Send message
        RestUserMessage sentMessage = await channel.SendMessageAsync(messageInputData.Value);
        
        //Save details
        server.RuleReactionMessageId = sentMessage.Id;
        await dbContext.SaveChangesAsync();

        await modal.RespondAsync(
            $"Message of ID **{sentMessage.Id}** in channel {channel.Mention} has been set as the rule reaction message.");
    }
}