using System;
using System.Diagnostics;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pootis_Bot.Core;
using Pootis_Bot.Services;
using Pootis_Bot.Services.Audio;
using Pootis_Bot.Shared.Logging;
using Serilog;
using Victoria;

//Create application
HostApplicationBuilder builder = Host.CreateApplicationBuilder();

//Setup logger
Logger logger = builder.Services.SetupLogger(builder.Configuration);

try
{
    //Setup Config
    PootisBotConfig pootisBotConfig = new();
    IConfigurationSection config = builder.Configuration.GetSection("Config");
    config.Bind(pootisBotConfig);
    builder.Services.Configure<PootisBotConfig>(config);
    
    //Setup Discord Config
    builder.Services.Configure<DiscordSocketConfig>(builder.Configuration.GetSection("DiscordConfig"));

    //Setup DB
    //builder.Services.UseVoltProjectDbContext(builder.Configuration, "Builder");

    builder.Services.AddSingleton<DiscordSocketClient>();
    builder.Services.AddSingleton<CommandHandler>();
    builder.Services.AddHostedService<BotClientService>();

    //Audio Services
    if (pootisBotConfig.EnableAudioServices)
    {
        builder.Services.AddSingleton<AudioSelectionService>();
        builder.Services.AddSingleton<AudioService>();
    
        //Extensions
        builder.Services.AddLavaNode(configuration =>
        {
            configuration = pootisBotConfig.VictoriaConfig;
        });
    }

    builder.Services.AddHttpClient();

    //Setup app
    IHost host = builder.Build();

    //Handle DB migrations
    //host.HandleDbMigrations();
    
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