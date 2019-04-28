using Discord;
using Discord.WebSocket;
using Pootis_Bot.Entities;
using System;
using System.Linq;

namespace Pootis_Bot.Core
{
    internal static class Global
    {
        //Here is a list of sites that Pootis-Bot can refer to, if your hosting your own version of the documentation you might wanna change it.
        internal static readonly string githubPage = "https://github.com/Creepysin/Pootis-Bot"; //Main github page

        internal static readonly string websiteHome = "https://pootis-bot.creepysin.com"; //Main docs page
        internal static readonly string websiteCommands = "https://pootis-bot.creepysin.com/commands/discord-commands/"; //Main Discord commands list
        internal static readonly string websiteServerSetup = "https://pootis-bot.creepysin.com/server-setup/"; //Main server-setup page
        internal static readonly string websiteConsoleCommands = "https://pootis-bot.creepysin.com/commands/console-commands/";

        //An array of Discord servers, add as many as you want and use it throught the bot.
                                                           // Main Server                -  Development Server
        internal static readonly string[] discordServers = { "https://discord.gg/m7hg47t", "https://discord.gg/m4YcsUa" };

        internal static readonly string version = "0.1 - Early Public Alpha";
        internal static readonly string aboutMessage = $"Pootis Bot --- | --- {version}\n" +
            $"Created by Creepysin licensed under the MIT license. Vist {githubPage}/blob/master/LICENSE.md for more info.\n\n" +
            $"Pootis Robot icon by Valve\n" +
            $"Created with Discord.NET\n" +
            $"https://github.com/Creepysin/Pootis-Bot \n\n" +
            $"Thank you for using Pootis Bot";

        internal static string botName;
        internal static string botPrefix;
        internal static string botToken;

        public static void WriteMessage(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{TimeNow()}] " + msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteMessage(string msg)
        {
            Console.WriteLine($"[{TimeNow()}] " + msg);
        }

        public static string TimeNow()
        {
            return DateTime.Now.ToString("h:mm:ss tt");
        }

        public static string Title(string s)
        {
            char[] array = s.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }

        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        public static IRole CheckIfRoleExist(SocketGuild guild, string rolename)
        {
            var result = from a in guild.Roles
                         where a.Name == rolename
                         select a;

            var role = result.FirstOrDefault();
            return role;
        }

        public static GlobalServerList.CommandInfo CheckCommand(string command, SocketGuild guild)
        {
            var server = ServerLists.GetServer(guild);
            var cmdinfo = server.GetCommandInfo(command);

            return cmdinfo;
        }

        public static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
        }
    }
}
