using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Discord.TypeConverters;
using Pootis_Bot.Modules;
using Emoji = Pootis_Bot.Core.Discord.Emoji;

namespace Pootis_Bot.Services.Core;

/// <summary>
///     Service for handling installation and management of Discord.Net command modules
/// </summary>
public sealed class CommandHandlerService
{
    private readonly DiscordSocketClient client;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<CommandHandlerService> logger;
    private readonly PootisBotConfig config;
    private readonly InteractionService interactionService;

    private readonly Type[] discordServices =
    [
        typeof(BasicModule),
        typeof(FunModule),
        typeof(HelpModule),
        typeof(ProfileModule),
        typeof(SetupModule)
    ];

    private readonly Type[] audioServices =
    [
        typeof(AudioModule)
    ];
    
    public CommandHandlerService(
        DiscordSocketClient client,
        IServiceProvider serviceProvider,
        ILogger<CommandHandlerService> logger,
        IOptions<PootisBotConfig> config)
    {
        this.client = client;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        this.config = config.Value;
        
        interactionService = new InteractionService(client);
        interactionService.AddTypeConverter<Emoji>(new EmojiTypeConverter());
        
        client.InteractionCreated += HandleInteraction;
    }

    public async Task InstallAssemblyModules(Assembly assembly)
    {
        //Install all services
        logger.LogInformation("Installing discord modules...");
        foreach (Type discordService in discordServices)
        {
            await interactionService.AddModuleAsync(discordService, serviceProvider);
        }
        
        //Install audio services
        if (config.EnableAudioServices)
        {
            logger.LogInformation("Installing audio module...");
            foreach (Type audioService in audioServices)
            {
                await interactionService.AddModuleAsync(audioService, serviceProvider);
            }
        }
        else
        {
            logger.LogWarning("Audio services are not enabled, not installing module.");
        }
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