using System;
using System.Collections.Generic;
using System.Text;

namespace Pootis_Bot.Core
{
    public class Global
    {
        public static void ColorMessage(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string TimeNow()
        {
            return DateTime.Now.ToString("h:mm:ss tt");
        }

    }
}
