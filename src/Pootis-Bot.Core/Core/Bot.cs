using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Pootis_Bot.Commands;
using Pootis_Bot.Config;
using Pootis_Bot.Console;
using Pootis_Bot.Console.ConfigMenus;
using Pootis_Bot.Exceptions;
using Pootis_Bot.Logging;
using Pootis_Bot.Modules;
using Serilog.Events;

namespace Pootis_Bot.Core;

/// <summary>
///     Main class for handling the bot
/// </summary>
public class Bot : IDisposable
{
	/// <summary>
	///     Is the loop for handling console commands running?
	/// </summary>
	private static bool isConsoleLoopRunning;

    /// <summary>
    ///     Command handler
    /// </summary>
    private CommandHandler commandHandler;

    /// <summary>
    ///     Config for the bot
    /// </summary>
    private BotConfig config;

    /// <summary>
    ///     Client for interacting with Discord
    /// </summary>
    private DiscordSocketClient discordClient;

    /// <summary>
    ///     Is this the bot's first ready
    /// </summary>
    private bool firstReady = true;

    /// <summary>
    ///     Handles calling to and managing installed modules
    /// </summary>
    private ModuleManager moduleManager;

    public Bot()
    {
        if (Instance != null)
            throw new InitializationException("There already is an instance of the bot!");

        Instance = this;
    }

    /// <summary>
    ///     Whether or not this bot is running
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    ///     The location of the application
    /// </summary>
    public static string ApplicationLocation { get; internal set; }

    /// <summary>
    ///     The bot instance
    /// </summary>
    public static Bot Instance { get; private set; }

    /// <summary>
    ///     Runs the bot
    /// </summary>
    /// <exception cref="InitializationException"></exception>
    public async Task Run()
    {
        //Don't want to be-able to run the bot multiple times
        if (IsRunning)
            throw new InitializationException("A bot is already running!");

        ApplicationLocation = Path.GetDirectoryName(typeof(Bot).Assembly.Location);

        IsRunning = true;

        //Init the logger
        Logger.Init();
        Logger.Info("Starting bot...");

        //Get config
        config = Config<BotConfig>.Instance;
        config.Saved += ConfigSaved;
        ConfigSaved();

        //Load modules
        moduleManager = new ModuleManager("Modules/", "Cache/NuGetAssemblies", "Cache/PackagesDownload");
        moduleManager.LoadModules();

        //If the token is null or white space, open the config menu
        if (string.IsNullOrWhiteSpace(config.BotToken))
        {
            Logger.Error("The token in the config is null or empty! You must set it in the config menu.");
            OpenConfigMenu();
        }

        //Setup the discord client
        discordClient = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
            GatewayIntents = config.GatewayIntents
        });
        discordClient.Log += Log;
        discordClient.Ready += Ready;

        //Log in and start the Discord client
        Logger.Info("Logging into Discord bot...");
        try
        {
            await discordClient.LoginAsync(TokenType.Bot, config.BotToken);
            await discordClient.StartAsync();
        }
        catch (HttpException)
        {
            Logger.Error("The supplied token was invalid!");
            Dispose();
            return;
        }

        Logger.Info("Login successful!");

        ModuleManager.ModulesClientConnected(discordClient);

        //Setup command handler
        commandHandler = new CommandHandler(discordClient);
        ModuleManager.InstallDiscordModulesFromLoadedModules(commandHandler);
        ModuleManager.InstallPermissionProvidersFromLoadedModules(commandHandler);
    }

    private void ConfigSaved()
    {
        System.Console.Title = config.BotName;
    }

    private async Task Ready()
    {
        ModuleManager.ModulesClientReady(discordClient, firstReady);
        firstReady = false;

        await commandHandler.RegisterInteractionCommands();

        Logger.Info("Bot is now ready and online!");
    }

    private Task Log(LogMessage message)
    {
        LogEventLevel severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };

        Logger.Write(severity, message.Exception, $"[{message.Source}] {message.Message}");

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Starts a console loop
    /// </summary>
    public static void ConsoleLoop()
    {
        ConsoleCommandManager.AddConsoleCommandsFromAssembly(typeof(Bot).Assembly);
        isConsoleLoopRunning = true;

        while (isConsoleLoopRunning)
        {
            string input = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                continue;

            ConsoleCommandManager.ExecuteCommand(input);
        }
    }

    #region Destroy

    ~Bot()
    {
        ReleaseResources();
    }

    /// <summary>
    ///     Disposes of this bot instance
    /// </summary>
    /// <exception cref="InitializationException"></exception>
    public void Dispose()
    {
        //The bot has already shutdown
        if (!IsRunning)
            throw new InitializationException("The bot has already shutdown!");

        ReleaseResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseResources()
    {
        isConsoleLoopRunning = false;

        discordClient.StopAsync().GetAwaiter().GetResult();
        discordClient.Dispose();

        moduleManager.Dispose();
        Logger.Shutdown();

        IsRunning = false;
        Instance = null;
    }

    #endregion

    #region Commands

    [ConsoleCommand("config", "Opens the config menu for the bot")]
    private static void ConfigMenuCommand()
    {
        OpenConfigMenu();
    }

    [ConsoleCommand("quit", "Quits running the bot")]
    private static void ShutdownBotCommand()
    {
        isConsoleLoopRunning = false;
    }

    private static void OpenConfigMenu()
    {
        BotConfig config = Config<BotConfig>.Instance;
        ConsoleConfigMenu<BotConfig> configMenu = new(config);

        while (true)
        {
            configMenu.Show();

            if (string.IsNullOrWhiteSpace(config.BotToken))
            {
                Logger.Error("The bot token is not set! Set the bot token then exit out of the menu.");
                continue;
            }

            break;
        }

        config.Save();
    }

    #endregion
}