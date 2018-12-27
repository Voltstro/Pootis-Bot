using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Pootis_Bot.Core;
using Discord;
using Pootis_Bot.Core.ServerList;
using Pootis_Bot.Core.UserAccounts;

namespace Pootis_Bot
{
    class Program
    {
        private bool isBotOn;

        DiscordSocketClient _client;
        CommandHandler _handler;

        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();


        public async Task StartAsync()
        {          
            if (Config.bot.botToken == "" || Config.bot.botToken == null) //Makes sure that the token is not null or empty
            {
                Console.WriteLine("The token was null or not present.");

                Config.SaveConfig(ServerConfigToken(), ServerConfigPrefix(), ServerConfigName(), null, null);
            }

            Console.Title = Config.bot.botName + " Console";

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            _client.Log += Log;
            _client.UserJoined += AnnounceJoinedUser;
            _client.UserLeft += UserLeft;
            _client.JoinedGuild += JoinedNewServer;
            await ConnectBot(); //Loging into the bot using the token in the config.
            await _client.StartAsync();
            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);
            await _client.SetGameAsync("Use $help for help.");
            ConsoleInput();
            await Task.Delay(-1);
        }

        private async Task ConnectBot()
        {
            if (isBotOn == false)
            {
                await _client.LoginAsync(TokenType.Bot, Config.bot.botToken);
                isBotOn = true;
            }
            else
                Console.WriteLine("Bot is already connected");
        }

        private async Task DisconnectBot()
        {
            if (isBotOn)
            {
                await _client.LogoutAsync();
                isBotOn = false;
            }
            else
                Console.WriteLine("Bot is already disconnected");
        }

        private async Task JoinedNewServer(SocketGuild arg)
        {
            Console.WriteLine("Joining server " + arg);
            ServerLists.GetServer(arg);
        }

        private async Task ConsoleInput()
        {
            var input = String.Empty;
            while (input.Trim().ToLower() != "block")
            {
                input = Console.ReadLine();
                if (input.Trim().ToLower() == "exit")
                {
                    Console.WriteLine("Shutting down...");
                    await _client.SetGameAsync("Bot shutting down");
                    await _client.LogoutAsync();
                    _client.Dispose();
                    Console.WriteLine("Press any key to quit...");
                    Console.ReadKey(true);
                    Environment.Exit(0);
                }
                else if(input.Trim().ToLower() == "disconnet")
                {
                    await DisconnectBot();
                }
                else if (input.Trim().ToLower() == "connect")
                {
                    await ConnectBot();
                }
            }
        }

        public async Task UserLeft(SocketGuildUser user) //Says goodbye to user.
        {
            Console.WriteLine($"User {user} has left the server.");

            var server = ServerLists.GetServer(user.Guild);

            if (server.enableWelcome == true)
            {
                if (!user.IsBot)
                {
                    var channel = _client.GetChannel(server.welcomeID) as SocketTextChannel; //gets channel to send message in
                    await channel.SendMessageAsync("Goodbye " + user.Mention + ". We hope you enjoyed your stay."); //Says goodbye.  
                }
            }
        }

        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
        }

        public async Task AnnounceJoinedUser(SocketGuildUser user) //welcomes New Players
        {
            Console.WriteLine($"User {user} has joined the server.");

            var server = ServerLists.GetServer(user.Guild);

            if (server.enableWelcome == true)
            {
                UserAccounts.GetAccount(user);
                var channel = _client.GetChannel(server.welcomeID) as SocketTextChannel; //gets channel to send message in
                await channel.SendMessageAsync("Welcome " + user.Mention + " to the Creepysin's Discord server! Consider checking out the rules first then enjoy your stay!"); //Welcomes the new user

            }
        }

        #region Server Config

        void ServerConfigMain()
        {
            if(isBotOn)
            {
                Console.WriteLine("Bot is on, disconnect the bot the change server config.");
                return;
            }
            else
            {
                Console.WriteLine("Server Config");
            }
        }

        string ServerConfigToken()
        {
            Console.WriteLine("Enter in your bot's token: ");
            string token = Console.ReadLine();
            return token;
        }

        string ServerConfigPrefix()
        {
            Console.WriteLine("Enter in your bot's prefix: ");
            string prefix = Console.ReadLine();
            return prefix;
        }

        string ServerConfigName()
        {
            Console.WriteLine("Enter in your bot's name: ");
            string name = Console.ReadLine();
            return name;
        }

        #endregion
    }
}
