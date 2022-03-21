using Pootis_Bot;
using Pootis_Bot.Core;
using Pootis_Bot.Helper;
using Spectre.Console;

//Ascii art of Pootis-Bot because why not ¯\_(ツ)_/¯
FigletFont font = FigletFont.Parse(Resources.StandardFont);
AnsiConsole.Write(new FigletText(font, "Pootis-Bot"));
AnsiConsole.MarkupLine($"        [bold]Version[/]: {VersionUtils.GetApplicationVersion()}");
AnsiConsole.Write("\n");

Bot bot = new();
bot.Run().GetAwaiter().GetResult();
Bot.ConsoleLoop();

bot.Dispose();