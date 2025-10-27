using System;
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

namespace Pootis_Bot.Services.Core.Client;

/// <summary>
///     Service for handling the bot client
/// </summary>
public class ClientService : IDisposable
{
    //Discord modules
    private readonly Type[] discordModules =
    [
        typeof(BasicModule),
        typeof(FunModule),
        typeof(HelpModule),
        typeof(ProfileModule),
        typeof(SetupModule)
    ];

    private readonly Type[] audioModules =
    [
        typeof(AudioModule)
    ];
    
    private readonly ILogger<ClientService> logger;
    private readonly PootisBotConfig botConfig;
    private readonly IServiceProvider serviceProvider;
    private readonly DiscordSocketClient client;
    
    private readonly InteractionService interactionService;

    private bool started;

    /// <summary>
    ///     The underlining <see cref="DiscordSocketClient"/>
    /// </summary>
    public DiscordSocketClient DiscordClient => client;
    
    public ClientService(
        ILogger<ClientService> logger,
        IOptions<PootisBotConfig> botConfig,
        IOptions<DiscordSocketConfig> socketClientConfig,
        IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.botConfig = botConfig.Value;
        
        //Check token is valid
        if (string.IsNullOrWhiteSpace(this.botConfig.BotToken))
            throw new NullReferenceException("BotToken in config cannot be null or whitespace!");
        
        //Create discord client
        DiscordSocketConfig socketConfig = socketClientConfig.Value;
        client = new DiscordSocketClient(socketConfig);
        
        client.Log += ClientOnLogMessage;
        client.Ready += ClientOnReady;
        
        //Create interaction service
        interactionService = new InteractionService(client);
        interactionService.AddTypeConverter<Emoji>(new EmojiTypeConverter());
        
        client.InteractionCreated += HandleInteraction;
        interactionService.SlashCommandExecuted += SlashCommandExecuted;
    }

    /// <summary>
    ///     Starts the client service
    /// </summary>
    public async Task Start()
    {
        if(started)
            throw new Exception("Bot is already started!");
        
        started = true;

        //Install command modules
        try
        {
            //Install all normal command modules
            logger.LogInformation("Installing command modules...");
            foreach (Type discordService in discordModules)
            {
                await interactionService.AddModuleAsync(discordService, serviceProvider);
            }
            
            //Install audio command module (if enabled)
            if (botConfig.EnableAudioServices)
            {
                logger.LogInformation("Installing audio command module...");
                foreach (Type audioService in audioModules)
                {
                    await interactionService.AddModuleAsync(audioService, serviceProvider);
                }
            }
            else
            {
                logger.LogWarning("Audio services are not enabled, not installing command module.");
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to install interaction modules!");
            throw;
        }
        
        //Log into Discord and start the client
        try
        {
            logger.LogInformation("Logging into Discord and starting client...");
            await client.LoginAsync(TokenType.Bot, botConfig.BotToken);
            await client.StartAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to start the Discord client!");
            throw;
        }
        
        logger.LogInformation("Successfully connected to Discord!");
    }

    public async Task Stop()
    {
        if(!started)
            throw new Exception("Bot is not started!");

        await client.StopAsync();
        
        started = false;
    }

    public void Dispose()
    {
        interactionService.Dispose();
        client.Dispose();
    }
    
    private async Task ClientOnReady()
    {
        logger.LogInformation("Discord client has signalled that it is ready. Registering commands....");

        try
        {
            if (botConfig.TestGuildId.HasValue)
            {
                await interactionService.RegisterCommandsToGuildAsync(botConfig.TestGuildId.Value);
            }
            else
            {
                await interactionService.RegisterCommandsGloballyAsync();
            }

        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to register commands!");
            return;
        }
        
        logger.LogInformation("Locked n loaded... bot is ready to go.");
    }
    
    private Task ClientOnLogMessage(LogMessage logMessage)
    {
        LogLevel logLevel = Utils.DiscordLogSeverityToLogLevel(logMessage.Severity);
        logger.Log(logLevel, logMessage.Exception, logMessage.Message);
        return Task.CompletedTask;
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
    
    private async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext ctx, IResult result)
    {
        if (result.IsSuccess || result.Error == null)
            return;

        switch (result.Error)
        {
            case InteractionCommandError.UnknownCommand: //Interactions shouldn't ever have this right?
                await ctx.Interaction.RespondAsync("Unknown Command!");
                break;
            case InteractionCommandError.ParseFailed:
            case InteractionCommandError.ConvertFailed:
            case InteractionCommandError.BadArgs:
                await ctx.Interaction.RespondAsync($"Command has bad arguments! {result.ErrorReason}");
                break;
            case InteractionCommandError.Exception:
            case InteractionCommandError.Unsuccessful:
                await ctx.Interaction.RespondAsync("Sorry, but an internal error occured while executing this command!");
                logger.LogError("An error occured while executing a command!\n{ResultErrorReason}", result.ErrorReason);
                break;
            case InteractionCommandError.UnmetPrecondition:
                await ctx.Interaction.RespondAsync("Sorry, but you don't meet the preconditions to run this command!");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}