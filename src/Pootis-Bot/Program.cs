using Pootis_Bot.Core;
using Pootis_Bot.Helper;

namespace Pootis_Bot;

public static class Program
{
    public static void Main(string[] args)
    {
        //Ascii art of Pootis-Bot because why not ¯\_(ツ)_/¯
        System.Console.WriteLine(@"__________              __  .__                 __________        __   ");
        System.Console.WriteLine(@"\______   \____   _____/  |_|__| ______         \______   \ _____/  |_ ");
        System.Console.WriteLine(@" |     ___/  _ \ /  _ \   __\  |/  ___/  ______  |    |  _//  _ \   __\");
        System.Console.WriteLine(@" |    |  (  <_> |  <_> )  | |  |\___ \  /_____/  |    |   (  <_> )  |  ");
        System.Console.WriteLine(@" |____|   \____/ \____/|__| |__/____  >          |______  /\____/|__|  ");
        System.Console.WriteLine(@"                                    \/                  \/             ");
        System.Console.WriteLine($"			Version: {VersionUtils.GetApplicationVersion()}");
        System.Console.WriteLine();

        Bot bot = new();
        bot.Run().GetAwaiter().GetResult();
        Bot.ConsoleLoop();

        bot.Dispose();
    }
}