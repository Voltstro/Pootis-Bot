using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Pootis_Bot.Core;
using Discord;
using Discord.Commands;

namespace Pootis_Bot
{
    class Program
    {
        static void Main(string[] args)
        => new Program().StartAsync(args).GetAwaiter().GetResult();

        public async Task StartAsync(string[] args)
        {
            string name = null, token = null, prefix = null;

            #region Config Loading

            if(args.Length != 0)
            {
                if(args.Length == 1)
                    token = args[0];
                if (args.Length == 2)
                    prefix = args[1];
                if (args.Length == 3)
                    name = args[2];
            }
            if (name == null) name = Config.bot.botName;
            if (token == null) token = Config.bot.botToken;
            if (prefix == null) prefix = Config.bot.botPrefix;

            #endregion

            Console.Title = name + " Console";
            Global.WriteMessage($"Starting...", ConsoleColor.White);

            Bot bot = new Bot(token, name, prefix);
            await bot.StartBot();
        }
    }
}
