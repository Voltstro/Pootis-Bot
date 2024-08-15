using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient client;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<CommandHandler> logger;
    private readonly PootisBotConfig config;
    private readonly InteractionService interactionService;
    
    public CommandHandler(
        DiscordSocketClient client,
        IServiceProvider serviceProvider,
        ILogger<CommandHandler> logger,
        IOptions<PootisBotConfig> config)
    {
        this.client = client;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        this.config = config.Value;
        
        interactionService = new InteractionService(client);
        //interactionService.SlashCommandExecuted += OnSlashCommandExecute;
        
        client.InteractionCreated += HandleInteraction;
    }

    public async Task InstallAssemblyModules(Assembly assembly)
    {
        await interactionService.AddModulesAsync(assembly, serviceProvider);
    }

    /// <summary>
    ///     Register commands with Guilds
    /// </summary>
    public async Task RegisterCommands()
    {
        if (config.TestGuildId.HasValue)
        {
            await interactionService.RegisterCommandsToGuildAsync(config.TestGuildId.Value);
            return;
        }

        await interactionService.RegisterCommandsGloballyAsync();
    }
    
    private Task OnSlashCommandExecute(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        throw new NotImplementedException();
    }
    
    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            SocketInteractionContext ctx = new(client, interaction);
            await interactionService.ExecuteCommandAsync(ctx, serviceProvider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling interaction!");

            //If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            //response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type == InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }

}