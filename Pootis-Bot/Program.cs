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
                //Token was null or empty, enter into bot config mode.
                Global.ColorMessage($"[{Global.TimeNow()}] The token was null or not present. Entering Bot Config Mode", ConsoleColor.Red);

                BotConfigStart();
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
            #pragma warning disable CS4014 //Ingnore this annoying warning
            ConsoleInput();
            #pragma warning restore CS4014
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

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Thanks for inviting me!");
            embed.WithDescription("Hello! My name is " + Config.bot.botName + "!\n\n**__Links__**" +
                "\n:computer: [Commands](https://github.com/CreepysinProjects/Pootis-Bot/wiki/Pootis-Bot-Commands)" +
                "\n<:GitHub:529571722991763456> [Github Page](https://github.com/CreepysinProjects/Pootis-Bot)" +
                "\n<:Discord:529572497130127360> [Creepysin Development Server](https://discord.gg/m4YcsUa)" +
                "\n<:Discord:529572497130127360> [Creepysin Server](https://discord.gg/m7hg47t)");
            embed.WithColor(new Color(241, 196, 15));


            await arg.DefaultChannel.SendMessageAsync("", false, embed.Build());
        }
        
        private async Task ConsoleInput()
        {
            var input = String.Empty;
            while (input.Trim().ToLower() != "block")
            {
                input = Console.ReadLine();
                if (input.Trim().ToLower() == "exit")
                {
                    Console.WriteLine($"[{Global.TimeNow()}] Shutting down...");
                    await _client.SetGameAsync("Bot shutting down");
                    await _client.LogoutAsync();
                    _client.Dispose();
                    Console.WriteLine("Press any key to quit...");
                    Console.ReadKey(true);
                    Environment.Exit(0);
                }
                else if (input.Trim().ToLower() == "config")
                {
                    BotConfigStart();
                    Console.WriteLine($"[{Global.TimeNow()}] Restart the bot to apply the settings");
                }
                else if(input.Trim().ToLower() == "setgame")
                {
                    Console.WriteLine("Enter in what you want to set the bot's game to.");
                    string set = Console.ReadLine();
                    await _client.SetGameAsync(set);
                    Console.WriteLine($"Bot's game was set to '{set}'");
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
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task Log(LogMessage msg)
        {
            Console.WriteLine($"[{Global.TimeNow()}] " + msg.Message);
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public async Task AnnounceJoinedUser(SocketGuildUser user) //welcomes New Players
        {
            Console.WriteLine($"User {user} has joined the server.");

            var server = ServerLists.GetServer(user.Guild);

            if (server.enableWelcome == true)
            {
                if(!user.IsBot)
                {
                    UserAccounts.GetAccount(user);
                    if (server.enableWelcome == false)
                        return;
                    var channel = _client.GetChannel(server.welcomeID) as SocketTextChannel; //gets channel to send message in
                    string rules = "";
                    if(server.isRules)
                    {
                        rules = "Consider checking out the #rules then enjoy your stay!";
                    }

                    await channel.SendMessageAsync("Welcome " + user.Mention + $" to the {user.Guild.Name}! {rules}"); //Welcomes the new user
                }
            }
        }

        #region Bot Config

        void BotConfigStart()
        {
            Console.WriteLine("");
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("                    Bot configuration                    ");
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("1. - Bot Token");
            Console.WriteLine("2. - Bot Prefix");
            Console.WriteLine("3. - Bot Name");
            Console.WriteLine("4. - APIs");
            Console.WriteLine("");
            Console.WriteLine("At any time type 'exit' to exit the bot configuration");
            BotConfigMain();
        }

        void BotConfigMain()
        {
            bool botConfig = true;

            string token = Config.bot.botToken;
            string name = Config.bot.botName;
            string prefix = Config.bot.botPrefix;

            if (token == null)
                Console.WriteLine("Token is null");
            else
                Console.WriteLine("token is: " + token);

            while(botConfig == true)
            {
                string input = Console.ReadLine();
                if(input.Trim().ToLower() == "exit")
                {
                    if (token != null)
                    {
                        if (name != null)
                        {
                            if (prefix != null)
                            {
                                botConfig = false;
                                Config.SaveConfig(token, prefix, name, Config.bot.apiGiphyKey, Config.bot.apiYoutubeKey, Config.bot.apiGoogleSearchKey, Config.bot.googleSearchEngineID);

                                Console.WriteLine("Exited bot configuration");
                                return;
                            }
                            else
                                Console.WriteLine("You need to set the bot prefix");
                        }
                        else
                            Console.WriteLine("You need to set the bot name");
                    }
                    else
                        Console.WriteLine("You need to set the bot token");       
                }
                else if(input.Trim().ToLower() == "1")
                {
                    token = BotConfigToken();
                }
                else if (input.Trim().ToLower() == "2")
                {
                    prefix = BotConfigPrefix();
                }
                else if (input.Trim().ToLower() == "3")
                {
                    name = BotConfigName();
                }
                else if (input.Trim().ToLower() == "4")
                {
                    BotConfigAPIs();
                }
            }
        }

        void BotConfigAPIs()
        {
            string giphyAPI = "";
            string youtubeAPI = "";
            string googleAPI = "";
            string googleSearchID = "";

            bool setAPIS = false;

            Console.WriteLine("APIs are needed for such commands as 'google'");
            Console.WriteLine("It is definitely recommened to set the API keys now.");
            Console.WriteLine("");
            Console.WriteLine("1. - Giphy API Key");
            Console.WriteLine("2. - Youtube API Key");
            Console.WriteLine("3. - Google API Key");
            Console.WriteLine("4. - Google Search ID");
            Console.WriteLine("");
            Console.WriteLine("At any time type 'return' to return back to the bot configuration menu.");

            while (setAPIS == false)
            {
                string input = Console.ReadLine();
                if (input.Trim().ToLower() == "return")
                {
                    Config.SaveConfig(Config.bot.botToken, Config.bot.botPrefix, Config.bot.botName, giphyAPI, youtubeAPI, googleAPI, googleSearchID);
                    Console.WriteLine("Exited api configuration");
                    return;
                }
                else if (input.Trim().ToLower() == "1")
                {
                    giphyAPI = BotConfigAPIGiphy();
                }
                else if (input.Trim().ToLower() == "2")
                {
                    youtubeAPI = BotConfigAPIYoutube();
                }
                else if (input.Trim().ToLower() == "3")
                {
                    googleAPI = BotConfigAPIGoogle();
                }
                else if (input.Trim().ToLower() == "4")
                {
                    googleSearchID = BotConfigGoogleSearchID();
                }
            }
        }

        #region Inputs

        string BotConfigToken()
        {
            string token = "";
            bool set = false;

            Console.WriteLine($"The current bot token is set to: '{Config.bot.botToken}'");
            Console.WriteLine("Enter in what you want to change the bot token to: ");

            while (set == false)
            {
                token = Console.ReadLine();
                if (token == "")
                {
                    Console.WriteLine("You cannot set the bot token to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's token was set to '{token}'");
                    set = true;
                }
            }

            return token;
        }

        string BotConfigPrefix()
        {
            string prefix = "";
            bool set = false;

            Console.WriteLine($"The current bot prefix is set to: '{Config.bot.botPrefix}'");
            Console.WriteLine("Enter in what you want to change the bot prefix to: ");

            while (set == false)
            {
                prefix = Console.ReadLine();
                if (prefix == "")
                {
                    Console.WriteLine("You cannot set the bot prefix to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's prefix was set to '{prefix}'");
                    set = true;
                }
            }

            return prefix;
        }

        string BotConfigName()
        {
            string name = "";
            bool set = false;

            Console.WriteLine($"The current bot name is set to: '{Config.bot.botName}'");
            Console.WriteLine("Enter in what you want to change the bot name to: ");

            while (set == false)
            {
                name = Console.ReadLine();
                if (name == "")
                {
                    Console.WriteLine("You cannot set the bot name to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's name was set to '{name}'");
                    set = true;
                }
            }

            return name;
        }

        string BotConfigAPIGiphy()
        {
            string key = "";
            bool set = false;

            Console.WriteLine($"The current bot Giphy key is set to: '{Config.bot.apiGiphyKey}'");
            Console.WriteLine("Enter in what you want to change the bot Giphy key to: ");

            while (set == false)
            {
                key = Console.ReadLine();
                if (key == "")
                {
                    Console.WriteLine("You cannot set the bot Giphy key to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's Giphy key was set to '{key}'");
                    set = true;
                }
            }

            return key;
        }

        string BotConfigAPIYoutube()
        {
            string key = "";
            bool set = false;

            Console.WriteLine($"The current bot Youtube key is set to: '{Config.bot.apiYoutubeKey}'");
            Console.WriteLine("Enter in what you want to change the bot Youtube key to: ");

            while (set == false)
            {
                key = Console.ReadLine();
                if (key == "")
                {
                    Console.WriteLine("You cannot set the bot Youtube key to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's Youtube key was set to '{key}'");
                    set = true;
                }
            }

            return key;
        }

        string BotConfigAPIGoogle()
        {
            string key = "";
            bool set = false;

            Console.WriteLine($"The current bot Google key is set to: '{Config.bot.apiGoogleSearchKey}'");
            Console.WriteLine("Enter in what you want to change the bot Google key to: ");

            while (set == false)
            {
                key = Console.ReadLine();
                if (key == "")
                {
                    Console.WriteLine("You cannot set the bot Google key to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's Google key was set to '{key}'");
                    set = true;
                }
            }

            return key;
        }

        string BotConfigGoogleSearchID()
        {
            string key = "";
            bool set = false;

            Console.WriteLine($"The current bot Google Search ID is set to: '{Config.bot.googleSearchEngineID}'");
            Console.WriteLine("Enter in what you want to change the bot Google Search ID to: ");

            while (set == false)
            {
                key = Console.ReadLine();
                if (key == "")
                {
                    Console.WriteLine("You cannot set the bot Google Search ID to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's Google Search ID was set to '{key}'");
                    set = true;
                }
            }

            return key;
        }

        #endregion

        #endregion
    }
}
