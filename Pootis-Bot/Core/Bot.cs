using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
    public class Bot
    {
        DiscordSocketClient _client;
        CommandService _commands;
        CommandHandler _handler;

        private bool isBotOn;

        private static string bottoken;
        public static string botname;
        public static string botprefix;

        public Bot(string _bottoken, string _botname, string _botprefix)
        {
            bottoken = _bottoken;
            botname = _botname;
            botprefix = _botprefix;
        }

        public async Task StartBot()
        {
            if (string.IsNullOrEmpty(bottoken))
            {
                BotConfigStart();
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            _client.Log += Log;
            _client.UserJoined += AnnounceJoinedUser;
            _client.UserLeft += UserLeft;
            _client.JoinedGuild += JoinedNewServer;
            _client.ReactionAdded += ReactionAdded;

            _commands = new CommandService();

            await ConnectBot(bottoken); //Loging into the bot using the token in the config.

            await _client.StartAsync();
            _handler = new CommandHandler(_client, _commands, botprefix);
            await _handler.InstallCommandsAsync();
            await _client.SetGameAsync("Use $help for help.");
            isBotOn = true;
#pragma warning disable CS4014 //Ingnore this annoying warning
            ConsoleInput();
#pragma warning restore CS4014
            await Task.Delay(-1);
        }

        private Task ReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            LevelingSystem.UserSentMessage((SocketGuildUser)reaction.User, (SocketTextChannel)reaction.Channel, 5);

            return Task.CompletedTask;
        }

        private async Task ConnectBot(string token)
        {
            if (isBotOn == false)
            {
                await _client.LoginAsync(TokenType.Bot, token);
                isBotOn = true;
            }
            else
                Global.WriteMessage("Bot is already connected", ConsoleColor.Yellow);
        }

        private async Task JoinedNewServer(SocketGuild arg)
        {
            Global.WriteMessage("Joining server " + arg, ConsoleColor.Blue);
            ServerLists.GetServer(arg);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Thanks for inviting me!");
            embed.WithDescription("Hello! My name is " + botname + "!\n\n**__Links__**" +
                "\n:computer: [Commands](https://creepysin.github.io/Pootis-Bot/commands/)" +
                "\n<:GitHub:529571722991763456> [Github Page](https://github.com/Creepysin/Pootis-Bot)" +
                "\n:bookmark: [Documation](https://creepysin.github.io/Pootis-Bot/)" +
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
                    Global.WriteMessage("Shutting down...", ConsoleColor.White);
                    await _client.SetGameAsync("Bot shutting down");
                    foreach (GlobalServerMusicItem channel in AudioService.CurrentChannels)
                    {
                        channel.AudioClient.Dispose();
                    }

                    await _client.LogoutAsync();
                    _client.Dispose();
                    Environment.Exit(0);
                }
                else if (input.Trim().ToLower() == "config")
                {
                    BotConfigStart();
                    Global.WriteMessage("Restart the bot to apply the settings", ConsoleColor.White);
                }
                else if (input.Trim().ToLower() == "setgame")
                {
                    Global.WriteMessage("Enter in what you want to set the bot's game to.", ConsoleColor.White);
                    string set = Console.ReadLine();
                    await _client.SetGameAsync(set);
                    Global.WriteMessage($"Bot's game was set to '{set}'", ConsoleColor.White);
                }
                else if (input.Trim().ToLower() == "deletemusic")
                {
                    foreach(GlobalServerMusicItem channel in AudioService.CurrentChannels)
                    {
                        channel.AudioClient.Dispose();
                    }

                    Global.WriteMessage("Deleting music directory...", ConsoleColor.Blue);
                    if (System.IO.Directory.Exists("Music/"))
                    {
                        System.IO.Directory.Delete("Music/", true);
                        Global.WriteMessage("Done!", ConsoleColor.Blue);
                    }
                    else
                        Global.WriteMessage("The music directory doesn't exist!", ConsoleColor.Blue);
                }
                else if (input.Trim().ToLower() == "toggleaudio")
                {
                    Config.bot.isAudioServiceEnabled = !Config.bot.isAudioServiceEnabled;
                    Config.SaveConfig();

                    Global.WriteMessage($"The audio service was set to {Config.bot.isAudioServiceEnabled}", ConsoleColor.Blue);
                    if (Config.bot.isAudioServiceEnabled == true)
                        Program.CheckAudioService();
                }
            }
        }

        private async Task UserLeft(SocketGuildUser user) //Says goodbye to user.
        {
            var server = ServerLists.GetServer(user.Guild);

            if (server.EnableWelcome == true)
            {
                if (!user.IsBot)
                {
                    var channel = _client.GetChannel(server.WelcomeID) as SocketTextChannel; //gets channel to send message in
                    await channel.SendMessageAsync("Goodbye " + user.Mention + ". We hope you enjoyed your stay."); //Says goodbye.  
                }
            }
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task Log(LogMessage msg)
        {
            Global.WriteMessage(msg.Message, ConsoleColor.White);
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        private async Task AnnounceJoinedUser(SocketGuildUser user) //welcomes New Players
        {
            Global.WriteMessage($"User {user} has joined the server {user.Guild.Name}({user.Guild.Id})", ConsoleColor.White);

            var server = ServerLists.GetServer(user.Guild);

            if (server.EnableWelcome == true)
            {
                if (!user.IsBot)
                {
                    UserAccounts.GetAccount(user);
                    if (server.EnableWelcome == false)
                        return;
                    var channel = _client.GetChannel(server.WelcomeID) as SocketTextChannel; //gets channel to send message in
                    string rules = "";
                    if (server.IsRules)
                    {
                        rules = server.RulesMessage;
                    }

                    await channel.SendMessageAsync("Welcome " + user.Mention + $" to the {user.Guild.Name}! {rules}"); //Welcomes the new user
                }
            }
        }

        #region Bot Config

        public void BotConfigStart()
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

            while (botConfig == true)
            {
                string input = Console.ReadLine();
                if (input.Trim().ToLower() == "exit")
                {
                    if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(prefix))
                    {
                        botConfig = false;
                        Config.bot.botToken = token;
                        Config.bot.botName = name;
                        Config.bot.botPrefix = prefix;

                        Config.SaveConfig();

                        bottoken = token;
                        botname = name;
                        botprefix = prefix;

                        Console.WriteLine("Exited bot configuration");
                        return;
                    }
                    else
                        Console.WriteLine("You need to set the token the name AND the prefix! One of them are null or empty");
                }
                else if (input.Trim().ToLower() == "1")
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
            string giphyAPI = Config.bot.apis.apiGiphyKey;
            string youtubeAPI = Config.bot.apis.apiYoutubeKey;
            string googleAPI = Config.bot.apis.apiGoogleSearchKey;
            string googleSearchID = Config.bot.apis.googleSearchEngineID;

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
                    Config.bot.apis.apiGiphyKey = giphyAPI;
                    Config.bot.apis.apiYoutubeKey = youtubeAPI;
                    Config.bot.apis.apiGoogleSearchKey = googleAPI;
                    Config.bot.apis.googleSearchEngineID = googleSearchID;

                    Config.SaveConfig();
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

            Console.WriteLine($"The current bot Giphy key is set to: '{Config.bot.apis.apiGiphyKey}'");
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

            Console.WriteLine($"The current bot Youtube key is set to: '{Config.bot.apis.apiYoutubeKey}'");
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

            Console.WriteLine($"The current bot Google key is set to: '{Config.bot.apis.apiGoogleSearchKey}'");
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

            Console.WriteLine($"The current bot Google Search ID is set to: '{Config.bot.apis.googleSearchEngineID}'");
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
