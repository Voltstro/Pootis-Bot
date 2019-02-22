using Discord;
using Discord.WebSocket;
using Pootis_Bot.Entities;
using System;
using System.Linq;

namespace Pootis_Bot.Core
{
    public class Global
    {
        public static void WriteMessage(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{TimeNow()}] " + msg);
            Console.ForegroundColor = ConsoleColor.White;
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

    }
}
