using System;
using System.Diagnostics;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Core;
using Pootis_Bot.Services.Audio;
using Pootis_Bot.Services.Core.Client;
using Pootis_Bot.Services.Interactions.Buttons;
using Pootis_Bot.Services.Interactions.Modal;
using Pootis_Bot.Services.Interactions.SelectMenu;
using Pootis_Bot.Services.Profile;
using Pootis_Bot.Services.Server;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Logging;
using Serilog;
using Victoria;
using WikiDotNet;

//Create application
HostApplicationBuilder builder = Host.CreateApplicationBuilder();

//Setup logger
Logger logger = builder.Services.SetupLogger(builder.Configuration);

try
{
    //Http Client
    builder.Services.AddHttpClient();
    
    //Setup Config
    PootisBotConfig pootisBotConfig = new();
    IConfigurationSection config = builder.Configuration.GetSection("Config");
    config.Bind(pootisBotConfig);
    builder.Services.Configure<PootisBotConfig>(config);
    
    //Install Discord client config
    builder.Services.Configure<DiscordSocketConfig>(builder.Configuration.GetSection("DiscordConfig"));
    
    //Core Pootis-Bot Services
    builder.Services.AddSingleton<ClientService>();
    builder.Services.AddHostedService<ClientBackgroundService>();

    builder.Services.AddSingleton<SelectMenuService>();
    builder.Services.AddSingleton<ButtonsService>();
    builder.Services.AddSingleton<ServerSetupService>();
    builder.Services.AddSingleton<ModalService>();
    
    //Background Services
    builder.Services.AddHostedService<ProfileXpBackgroundService>();
    builder.Services.AddHostedService<ServerRuleReactionBackgroundService>();
    builder.Services.AddHostedService<ServerWelcomeGoodbyeBackgroundService>();
    builder.Services.AddHostedService<ServerSetupBackgroundService>();

    //Audio Services
    if (pootisBotConfig.EnableAudioServices)
    {
        //Extensions
        builder.Services.AddSingleton<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>(provider =>
        {
            ClientService clientService = provider.GetRequiredService<ClientService>();
            
            ILogger<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>> lavaNodeLogger = provider.GetRequiredService<ILogger<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>>();
            return new LavaNode(clientService.DiscordClient, pootisBotConfig.VictoriaConfig, lavaNodeLogger);
        });
        
        builder.Services.AddSingleton<AudioService>();
    }
    
    //Other
    builder.Services.AddSingleton<WikiSearcher>();
    
    //Setup DB
    builder.Services.UsePootisBotDbContext(builder.Configuration, "Pootis");

    //Setup app
    IHost host = builder.Build();

    //Handle DB migrations
    host.HandleDbMigrations();
    
    //Start
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Error(ex, "An uncaught error occured!");
#if DEBUG
    if (Debugger.IsAttached)
        throw;
#endif
    
    return 1;
}
finally
{
    logger.Dispose();
}

return 0;