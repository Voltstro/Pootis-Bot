using System;
using System.Threading.Tasks;
using Pootis_Bot.Core;
using Pootis_Bot.Helper;
using Pootis_Bot.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pootis_Bot;

internal static class Program
{
    public static void Main(string[] args)
    {
        CommandApp app = new CommandApp();
        app.SetDefaultCommand<BaseCommand>();
        app.Run(args);
    }

    private sealed class BaseCommand : Command<BaseCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [CommandOption("--headless")]
            public bool Headless { get; init; }
        }
        
        public override int Execute(CommandContext context, Settings settings)
        {
            //Ascii art of Pootis-Bot because why not ¯\_(ツ)_/¯
            FigletFont font = FigletFont.Parse(Resources.StandardFont);
            AnsiConsole.Write(new FigletText(font, "Pootis-Bot"));
            AnsiConsole.MarkupLine($"        [bold]Version[/]: {VersionUtils.GetApplicationVersion()}");
            AnsiConsole.Write("\n");
            
            RunBot(settings).GetAwaiter().GetResult();
            return 0;
        }

        private async Task<int> RunBot(Settings settings)
        {
            Bot bot = new(new BotSettings
            {
                Headless = settings.Headless
            });

            Logger.Init();
            try
            {
                await bot.Run();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occured during startup!");
                return 1;
            }
            
            if(!settings.Headless)
                Bot.ConsoleLoop();

            bot.Dispose();
            return 0;
        }
    }
}