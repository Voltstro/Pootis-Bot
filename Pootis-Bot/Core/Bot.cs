using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Entities;
using Pootis_Bot.Services;

namespace Pootis_Bot.Core
{
    public class Bot
    {
        DiscordSocketClient _client;

        private string gameStatus = Config.bot.gameMessage;
        private bool isStreaming;

        public async Task StartBot()
        {
            isStreaming = false;

            if (string.IsNullOrEmpty(Global.botToken))
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
            _client.Ready += BotReadyAsync;
            await _client.LoginAsync(TokenType.Bot, Global.botToken); //Loging into the bot using the token in the config.

            await _client.StartAsync();
            CommandService _commands = new CommandService();
            CommandHandler _handler = new CommandHandler(_client, _commands);
            await _handler.InstallCommandsAsync();
            await _client.SetGameAsync(gameStatus);
            
            await Task.Delay(-1);
        }

        private async Task BotReadyAsync()
        {
            //Check the current connected server settings
            await CheckConnectedServerSettings();
            Global.Log("Bot is now ready and online");
            ConsoleInput();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task Log(LogMessage msg)
        {
            Global.Log(msg.Message);
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        private async Task CheckConnectedServerSettings()
        {
            Global.Log("Checking pre-connected server settings...");

            bool somethingChanged = false;
            int changeCount = 0;

            foreach(var server in ServerLists.serverLists)
            {
                if(_client.GetChannel(server.WelcomeChannel) == null && server.WelcomeMessageEnabled)
                {
                    somethingChanged = true;
                    changeCount++;

                    var guild = _client.GetGuild(server.ServerID);
                    var ownerDM = await guild.Owner.GetOrCreateDMChannelAsync();

                    await ownerDM.SendMessageAsync($"{guild.Owner.Mention}, your server **{guild.Name}** welcome channel has been disabled due to that it no longer exist since the last bot up time.\n" +
                        $"You can enable it again with `{Config.bot.botPrefix}setupwelcome` command and your existing message should stay. ");

                    server.WelcomeMessageEnabled = false;
                    server.WelcomeChannel = 0;
                }
            }

            //If a server was updated then save the serverlist file
            if (somethingChanged)
            {
                ServerLists.SaveServerList();
                Global.Log(changeCount + " server settings are no longer vaild, there owners have been notified.");
            }
            else
                Global.Log("All servers are good");
        }

        private Task ReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var guild = (channel as SocketGuildChannel).Guild;
            var server = ServerLists.GetServer(guild);
            
            if(reaction.MessageId == server.RuleMessageID) //Check to see if the reaction is on the right message
            {
                if(server.RuleEnabled) //Check to see if the server even has to rule reaction enabled
                {
                    if (reaction.Emote.Name == server.RuleReactionEmoji) //If the person reacted with the right emoji then give them the role
                    {
                        var role = guild.Roles.FirstOrDefault(x => x.Name == server.RuleRole);

                        var user = (SocketGuildUser)reaction.User;
                        user.AddRoleAsync(role);
                    }
                }
            }
            else
            {
                if(VoteGivewayService.isVoteRunning) // If there is a vote going on then check to make sure the reaction doesn't have anything to do with that.
                {
                    foreach (var vote in VoteGivewayService.votes)
                    {
                        if(reaction.MessageId == vote.VoteMessageID)
                        {
                            if(reaction.Emote.Name == vote.YesEmoji)
                            {
                                vote.YesCount++;
                            }
                                
                            else if(reaction.Emote.Name == vote.NoEmoji)
                            {
                                vote.NoCount++;
                            }  
                        }
                    }
                }
                else
                {
                    if (!((SocketGuildUser)reaction.User).IsBot)
                        LevelingSystem.UserSentMessage((SocketGuildUser)reaction.User, (SocketTextChannel)reaction.Channel, 5);
                }
            }

            return Task.CompletedTask;
        }

        private async Task JoinedNewServer(SocketGuild arg)
        {
            Global.Log("Joining server " + arg, ConsoleColor.Blue);
            ServerLists.GetServer(arg);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Hey, thanks for adding me to your server.");
            embed.WithDescription("Hello! My name is " + Global.botName + "!\n\n**__Links__**" +
                $"\n:computer: [Commands]({Global.websiteCommands})" +
                $"\n<:GitHub:529571722991763456> [Github Page]({Global.githubPage})" +
                $"\n:bookmark: [Documation]({Global.websiteHome})" +
                $"\n<:Discord:529572497130127360> [Creepysin Development Server]({Global.discordServers[1]})" +
                $"\n<:Discord:529572497130127360> [Creepysin Server]({Global.discordServers[0]})" +
                "\n\nIf you have any issues the best place to ask for assistance is on the Creepysin Server!");
            embed.WithColor(new Color(241, 196, 15));

            //Send a message to the server's default channel with the hello message
            await arg.DefaultChannel.SendMessageAsync("", false, embed.Build());

            //Send a message to Discord server's owner about seting up the bot
            var owner = await arg.Owner.GetOrCreateDMChannelAsync();
            await owner.SendMessageAsync($"Thanks for using {Global.botName}! Check out {Global.websiteServerSetup} on how to setup {Global.botName} for your server.");
        }

        private async Task UserLeft(SocketGuildUser user) //Says goodbye to the user.
        {
            var server = ServerLists.GetServer(user.Guild);

            if (server.WelcomeMessageEnabled)
            {
                if (!user.IsBot)
                {
                    var channel = _client.GetChannel(server.WelcomeChannel) as SocketTextChannel; //gets channel to send message in

                    string addUserMetion = server.WelcomeGoodbyeMessage.Replace("[user]", user.Mention);

                    await channel.SendMessageAsync(addUserMetion); //Says goodbye.  
                }
            }
        }

        private async Task AnnounceJoinedUser(SocketGuildUser user) //welcomes New Players
        {
            Global.Log($"User {user} has joined the server {user.Guild.Name}({user.Guild.Id})", ConsoleColor.White);

            var server = ServerLists.GetServer(user.Guild);

            if (server.WelcomeMessageEnabled == true)
            {
                if (!user.IsBot)
                {
                    UserAccounts.GetAccount(user);
                    if (server.WelcomeMessageEnabled == false)
                        return;

                    var channel = (ISocketMessageChannel)_client.GetChannel(server.WelcomeChannel);
                
                    string addUserMetion = server.WelcomeMessage.Replace("[user]", user.Mention);
                    string addServerName = addUserMetion.Replace("[server]", user.Guild.Name);

                    await channel.SendMessageAsync(addServerName); //Welcomes the new user with the server's message
                }
            }
        }

        private async void ConsoleInput()
        {
            while (true)    // Run forever
            {
                string input = Console.ReadLine().Trim().ToLower();

                if (input == "exit")
                {
                    Global.Log("Shutting down...");
                    await _client.SetGameAsync("Bot shutting down");
                    foreach (GlobalServerMusicItem channel in AudioService.CurrentChannels)
                    {
                        channel.AudioClient.Dispose();
                    }

                    await _client.LogoutAsync();
                    _client.Dispose();
                    Environment.Exit(0);
                }
                else if (input == "config")
                {
                    BotConfigStart();
                    Global.Log("Restart the bot to apply the settings");
                }
                else if (input == "about")
                    Console.WriteLine(Global.aboutMessage);
                else if (input == "version")
                    Console.WriteLine(Global.version);
                else if (input == "setgame")
                {
                    Console.WriteLine("Enter in what you want to set the bot's game to: ");
                    gameStatus = Console.ReadLine();

                    ActivityType activity = ActivityType.Playing;
                    string twich = null;
                    if (isStreaming)
                    {
                        activity = ActivityType.Streaming;
                        twich = Config.bot.twichStreamingSite;
                    }  

                    await _client.SetGameAsync(gameStatus, twich, activity);

                    Global.Log($"Bot's game was set to '{gameStatus}'");
                }
                else if (input == "togglestream")
                {
                    if (isStreaming)
                    {
                        isStreaming = false;
                        await _client.SetGameAsync(gameStatus, null, ActivityType.Playing);
                        Global.Log("Bot is no longer streaming");
                    }
                    else
                    {
                        isStreaming = true;
                        await _client.SetGameAsync(gameStatus, Config.bot.twichStreamingSite, ActivityType.Streaming);
                        Global.Log("Bot is streaming");
                    }
                }
                else if (input == "deletemusic")
                {
                    foreach (GlobalServerMusicItem channel in AudioService.CurrentChannels)
                    {
                        channel.AudioClient.Dispose();
                    }

                    Global.Log("Deleting music directory...", ConsoleColor.Blue);
                    if (System.IO.Directory.Exists("Music/"))
                    {
                        System.IO.Directory.Delete("Music/", true);
                        Global.Log("Done!", ConsoleColor.Blue);
                    }
                    else
                        Global.Log("The music directory doesn't exist!", ConsoleColor.Blue);
                }
                else if (input == "toggleaudio")
                {
                    Config.bot.isAudioServiceEnabled = !Config.bot.isAudioServiceEnabled;
                    Config.SaveConfig();

                    Global.Log($"The audio service was set to {Config.bot.isAudioServiceEnabled}", ConsoleColor.Blue);
                    if (Config.bot.isAudioServiceEnabled == true)
                        Program.CheckAudioService();
                }
                else if (input == "forceaudioupdate")
                {
                    Global.Log("Updating audio files.", ConsoleColor.Blue);
                    foreach (GlobalServerMusicItem channel in AudioService.CurrentChannels)
                    {
                        channel.AudioClient.Dispose();
                    }

                    //Delete old files first
                    System.IO.Directory.Delete("External/", true);
                    System.IO.File.Delete("libsodium.dll");
                    System.IO.File.Delete("opus.dll");

                    Program.UpdateAudioFiles();
                    Global.Log("Audio files were updated.", ConsoleColor.Blue);
                }
                else if (input == "clear" || input == "cls")
                {
                    Console.Clear();
                }
                else
                    Global.Log($"Unknown command '{input}'. Vist {Global.websiteConsoleCommands} for a list of console commands.", ConsoleColor.Red);
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
            string token = Global.botToken;
            string name = Global.botName;
            string prefix = Global.botPrefix;

            while (true)
            {
                string input = Console.ReadLine().Trim();

                if (input == "exit")
                {
                    if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(prefix))
                    {
                        Config.bot.botToken = token;
                        Config.bot.botName = name;
                        Config.bot.botPrefix = prefix;

                        Config.SaveConfig();

                        Global.botToken = token;
                        Global.botName = name;
                        Global.botPrefix = prefix;

                        Console.WriteLine("Exited bot configuration");
                        return;
                    }
                    else
                        Console.WriteLine("Either the token, name or prefix is null or empty. Make sure to check that the all have data in it.");
                }
                else if (input == "1")
                {
                    token = BotConfigToken();
                }
                else if (input == "2")
                {
                    prefix = BotConfigPrefix();
                }
                else if (input == "3")
                {
                    name = BotConfigName();
                }
                else if (input == "4")
                {
                    BotConfigAPIs();
                }
                else
                    Console.WriteLine("Invaild input, you need to either enter '1', '2', '3', '4' or 'exit' (With out '')");
            }
        }

        void BotConfigAPIs()
        {
            string giphyAPI = Config.bot.apis.apiGiphyKey;
            string youtubeAPI = Config.bot.apis.apiYoutubeKey;
            string googleAPI = Config.bot.apis.apiGoogleSearchKey;
            string googleSearchID = Config.bot.apis.googleSearchEngineID;

            Console.WriteLine("APIs are needed for commands such as 'google'");
            Console.WriteLine("It is definitely recommended.");
            Console.WriteLine("");
            Console.WriteLine("1. - Giphy API Key");
            Console.WriteLine("2. - Youtube API Key");
            Console.WriteLine("3. - Google API Key");
            Console.WriteLine("4. - Google Search ID");
            Console.WriteLine("");
            Console.WriteLine("At any time type 'return' to return back to the bot configuration menu.");

            while (true)
            {
                string input = Console.ReadLine().Trim();

                if (input.ToLower() == "return")
                {
                    Config.bot.apis.apiGiphyKey = giphyAPI;
                    Config.bot.apis.apiYoutubeKey = youtubeAPI;
                    Config.bot.apis.apiGoogleSearchKey = googleAPI;
                    Config.bot.apis.googleSearchEngineID = googleSearchID;

                    Config.SaveConfig();
                    Console.WriteLine("Exited api configuration");
                    return;
                }
                else if (input == "1")
                {
                    giphyAPI = BotConfigAPIGiphy();
                }
                else if (input == "2")
                {
                    youtubeAPI = BotConfigAPIYoutube();
                }
                else if (input == "3")
                {
                    googleAPI = BotConfigAPIGoogle();
                }
                else if (input == "4")
                {
                    googleSearchID = BotConfigGoogleSearchID();
                }
                else
                    Console.WriteLine("You need to either put in '1', '2' ... etc or 'return'. (With out '')");
            }
        }

        #region Inputs

        string BotConfigToken()
        {
            string token;

            Console.WriteLine($"The current bot token is set to: '{Config.bot.botToken}'");
            Console.WriteLine("Enter in what you want to change the bot token to: ");

            while (true)
            {
                token = Console.ReadLine();
                if (token == "")
                {
                    Console.WriteLine("You cannot set the bot token to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's token was set to '{token}'");
                    return token;
                }
            }
        }

        string BotConfigPrefix()
        {
            string prefix;

            Console.WriteLine($"The current bot prefix is set to: '{Config.bot.botPrefix}'");
            Console.WriteLine("Enter in what you want to change the bot prefix to: ");

            while (true)
            {
                prefix = Console.ReadLine();
                if (prefix == "")
                {
                    Console.WriteLine("You cannot set the bot prefix to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's prefix was set to '{prefix}'");
                    return prefix;
                }
            }

            
        }

        string BotConfigName()
        {
            string name;

            Console.WriteLine($"The current bot name is set to: '{Config.bot.botName}'");
            Console.WriteLine("Enter in what you want to change the bot name to: ");

            while (true)
            {
                name = Console.ReadLine();
                if (name == "")
                {
                    Console.WriteLine("You cannot set the bot name to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's name was set to '{name}'");
                    return name;
                }
            }
        }

        string BotConfigAPIGiphy()
        {
            string key;

            Console.WriteLine($"The current bot Giphy key is set to: '{Config.bot.apis.apiGiphyKey}'");
            Console.WriteLine("Enter in what you want to change the bot Giphy key to: ");

            while (true)
            {
                key = Console.ReadLine();
                if (key == "")
                {
                    Console.WriteLine("You cannot set the bot Giphy key to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's Giphy key was set to '{key}'");
                    return key;
                }
            }
        }

        string BotConfigAPIYoutube()
        {
            string key;

            Console.WriteLine($"The current bot Youtube key is set to: '{Config.bot.apis.apiYoutubeKey}'");
            Console.WriteLine("Enter in what you want to change the bot Youtube key to: ");

            while (true)
            {
                key = Console.ReadLine();
                if (key == "")
                {
                    Console.WriteLine("You cannot set the bot Youtube key to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's Youtube key was set to '{key}'");
                    return key;
                }
            }
        }

        string BotConfigAPIGoogle()
        {
            string key;

            Console.WriteLine($"The current bot Google key is set to: '{Config.bot.apis.apiGoogleSearchKey}'");
            Console.WriteLine("Enter in what you want to change the bot Google key to: ");

            while (true)
            {
                key = Console.ReadLine();
                if (key == "")
                {
                    Console.WriteLine("You cannot set the bot Google key to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's Google key was set to '{key}'");
                    return key;
                }
            }
        }

        string BotConfigGoogleSearchID()
        {
            string key;

            Console.WriteLine($"The current bot Google Search ID is set to: '{Config.bot.apis.googleSearchEngineID}'");
            Console.WriteLine("Enter in what you want to change the bot Google Search ID to: ");

            while (true)
            {
                key = Console.ReadLine();
                if (key == "")
                {
                    Console.WriteLine("You cannot set the bot Google Search ID to blank!");
                }
                else
                {
                    Console.WriteLine($"Bot's Google Search ID was set to '{key}'");
                    return key;
                }
            }
        }

        #endregion

        #endregion
    }
}
