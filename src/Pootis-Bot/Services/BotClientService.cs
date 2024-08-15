using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services;

/// <summary>
///     Service for running the bot client
/// </summary>
public sealed class BotClientService : BackgroundService
{
    private readonly PootisBotConfig config;
    private readonly ILogger<BotClientService> logger;
    private readonly DiscordSocketClient client;
    private readonly CommandHandler commandHandler;

    public BotClientService(
        IOptions<PootisBotConfig> config,
        ILogger<BotClientService> logger,
        DiscordSocketClient client,
        CommandHandler commandHandler)
    {
        this.config = config.Value;
        this.logger = logger;
        this.client = client;
        this.commandHandler = commandHandler;

        //Check token is valid
        if (string.IsNullOrWhiteSpace(this.config.BotToken))
            throw new NullReferenceException("BotToken in config cannot be null or whitespace!");
        
        client.Log += ClientOnLogMessage;
        client.Ready += ClientOnRead;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await commandHandler.InstallAssemblyModules(typeof(Program).Assembly);
        
        logger.LogInformation("Logging into Discord and starting client...");
        await client.LoginAsync(TokenType.Bot, config.BotToken);
        await client.StartAsync();

        //Wait indefinably, until cancel token raises
        await Task.Delay(-1, stoppingToken);

        //Quit
        logger.LogInformation("Logging out and stopping client...");
        await client.StopAsync();
        await client.LogoutAsync();
    } 
    
    private async Task ClientOnRead()
    {
        logger.LogInformation("Discord client is ready.");

        await commandHandler.RegisterCommands();
    }
    
    private Task ClientOnLogMessage(LogMessage logMessage)
    {
        LogLevel logLevel = Utils.DiscordLogSeverityToLogLevel(logMessage.Severity);
        logger.Log(logLevel, logMessage.Exception, logMessage.Message);
        return Task.CompletedTask;
    }
}