using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Discord;
using Discord.WebSocket;
using Discord.Rest;
using Pootis_Bot.Entities;
using Pootis_Bot.Services;
using Pootis_Bot.Services.Audio;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Core
{
    public class Bot
    {
        private DiscordSocketClient _client;

        private string _gameStatus = Config.bot.GameMessage;
        private bool _isStreaming;
        private bool _isRunning;

        public async Task StartBot()
        {
            _isStreaming = false;
            _isRunning = true;

            //Make sure the token isn't null or empty, if so open the bot config menu.
            if (string.IsNullOrEmpty(Global.BotToken))
            {
                BotConfigStart();
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });

            //Setup client events
            _client.Log += Log;
            _client.UserJoined += AnnounceJoinedUser;
            _client.UserLeft += UserLeft;
            _client.JoinedGuild += JoinedNewServer;
            _client.ReactionAdded += ReactionAdded;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            _client.ChannelDestroyed += ChannelDestroyed;
            _client.Ready += BotReadyAsync;

            await _client.LoginAsync(TokenType.Bot, Global.BotToken); //Logging into the bot using the token in the config.
            await _client.StartAsync(); //Start the client
            CommandHandler handler = new CommandHandler(_client);

            //Install all the Modules
            await handler.InstallCommandsAsync();

            //Set the bot status to the default game status
            await _client.SetGameAsync(_gameStatus);
            await CheckConnectionStatus();
        }

        private Task ChannelDestroyed(SocketChannel channel)
        {
            var serverList = ServerLists.GetServer((channel as SocketGuildChannel).Guild);
            var voiceChannel = serverList.GetVoiceChannel(channel.Id);

            //If the channel deleted was an auto voice channel, remove it from the list.
            if(voiceChannel.Name != null)
            {
                serverList.VoiceChannels.Remove(voiceChannel);
                ServerLists.SaveServerList();
            }  

            return Task.CompletedTask;
        }

        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            GlobalServerList server = ServerLists.GetServer((user as SocketGuildUser).Guild);

            //If we are adding an auto voice channel
            if (after.VoiceChannel != null)
            {
                var voiceChannel = server.GetVoiceChannel(after.VoiceChannel.Id);
                if (voiceChannel.Name != null)
                {
                    RestVoiceChannel createdChannel = await after.VoiceChannel.Guild.CreateVoiceChannelAsync($"New {voiceChannel.Name} chat");

                    int count = server.ActiveAutoVoiceChannels.Count + 1;
                    await createdChannel.ModifyAsync(x =>
                    {
                        x.Bitrate = after.VoiceChannel.Bitrate;
                        x.Name = voiceChannel.Name + " #" + count;
                        x.CategoryId = after.VoiceChannel.CategoryId;
                    });

                    await (user as SocketGuildUser).ModifyAsync(x =>
                    {
                        x.ChannelId = createdChannel.Id;
                    });

                    server.ActiveAutoVoiceChannels.Add(createdChannel.Id);
                    ServerLists.SaveServerList();
                }
            }

            //If we are removing an auto voice channel
            if(before.VoiceChannel != null)
            {
                ulong activeChannel = server.GetActiveVoiceChannel(before.VoiceChannel.Id);
                if (activeChannel != 0)
                {
                    //There are no user on the active auto voice channel
                    if (before.VoiceChannel.Users.Count == 0)
                    {
                        await before.VoiceChannel.DeleteAsync();
                        server.ActiveAutoVoiceChannels.Remove(before.VoiceChannel.Id);
                        ServerLists.SaveServerList();
                    }
                }
            }

            //Only check channel user count if the audio services are enabled.
            if (Config.bot.IsAudioServiceEnabled)
            {
                List<GlobalServerMusicItem> toRemove = new List<GlobalServerMusicItem>();

                foreach (GlobalServerMusicItem channel in AudioService.CurrentChannels)
                {
                    if (channel.AudioChannel.Users.Count == 1)
                    {
                        //Stop ffmpeg if it is running
                        if(channel.FFmpeg != null)
                            channel.FFmpeg.Dispose();

                        //Leave the audio channel
                        await channel.AudioClient.StopAsync();

                        await channel.StartChannel.SendMessageAsync(":musical_note: Left the audio channel due to there being no one there :(");

                        toRemove.Add(channel);
                    }
                }

                //To avoid System.InvalidOperationException exception remove the channels after the foreach loop.
                if (toRemove.Count != 0)
                {
                    foreach (var channel in toRemove)
                    {
                        AudioService.CurrentChannels.Remove(channel);
                    }
                }
            }
        }

        private async Task BotReadyAsync()
        {
            //Check the current connected server settings
            await CheckConnectedServerSettings();
            Global.Log("Bot is now ready and online!");

            ConsoleInput();
        }

        private static Task Log(LogMessage msg)
        {
            Global.Log(msg.Message);
            return Task.CompletedTask;
        }

        private async Task CheckConnectionStatus()
        {
            while(_isRunning)
            {
                if (Config.bot.CheckConnectionStatus) // It is enabled then check the connection status ever so milliseconds
                {
                    await Task.Delay(Config.bot.CheckConnectionStatusInterval);
                    if (_client.ConnectionState == ConnectionState.Disconnected || _client.ConnectionState == ConnectionState.Disconnecting && _isRunning)
                    {
                        Global.Log("The bot had disconnect for some reason, restarting...", ConsoleColor.Yellow);

                        await _client.LogoutAsync();
                        _client.Dispose();

                        ProcessStartInfo newPootisStart = new ProcessStartInfo("dotnet", "Pootis-Bot.dll");
                        Process newPootis = new Process
                        {
                            StartInfo = newPootisStart
                        };
                        newPootis.Start();
                        Environment.Exit(0);
                    }
                } 
                else
                    await Task.Delay(-1); // Just run forever
            }
        }

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
                    var ownerDm = await guild.Owner.GetOrCreateDMChannelAsync();

                    await ownerDm.SendMessageAsync($"{guild.Owner.Mention}, your server **{guild.Name}** welcome channel has been disabled due to that it no longer exist since the last bot up time.\n" +
                        $"You can enable it again with `{Global.BotPrefix}setupwelcomemessage` command and your existing message should stay.");

                    server.WelcomeMessageEnabled = false;
                    server.WelcomeChannel = 0;
                }

                //Check to see if all the active channels don't have someone in it.
                List<ulong> deleteActiveChannels = new List<ulong>();

                foreach(var activeChannel in server.ActiveAutoVoiceChannels)
                {
                    if(_client.GetChannel(activeChannel).Users.Count == 0)
                    {
                        await (_client.GetChannel(activeChannel) as SocketVoiceChannel).DeleteAsync();
                        deleteActiveChannels.Add(activeChannel);
                        somethingChanged = true;
                    }
                }

                //Check to see if all the auto voice channels are there
                List<VoiceChannel> deleteAutoChannels = new List<VoiceChannel>();
                foreach(var autoChannel in server.VoiceChannels)
                {
                    if (_client.GetChannel(autoChannel.ID) == null)
                    {
                        deleteAutoChannels.Add(autoChannel);
                        somethingChanged = true;
                    }
                }

                //To avoid System.InvalidOperationException remove all of the objects from the list after
                foreach (var activeChannel in deleteActiveChannels)
                {
                    server.ActiveAutoVoiceChannels.Remove(activeChannel);
                }

                foreach(var autoChannel in deleteAutoChannels)
                {
                    server.VoiceChannels.Remove(autoChannel);
                }
            }

            //If a server was updated then save the ServerList.json file
            if (somethingChanged)
            {
                ServerLists.SaveServerList();
            }
            else
                Global.Log("All servers are good!");
        }

        private Task ReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var guild = (channel as SocketGuildChannel).Guild;
            var server = ServerLists.GetServer(guild);
            
            if(reaction.MessageId == server.RuleMessageId) //Check to see if the reaction is on the right message
            {
                if(server.RuleEnabled) //Check to see if the server even has to rule reaction enabled
                {
                    if(reaction.Emote.Name == server.RuleReactionEmoji) //If the person reacted with the right emoji then give them the role
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

        private async Task JoinedNewServer(SocketGuild guild)
        {
            Global.Log("Joined server " + guild, ConsoleColor.Blue);
            ServerLists.GetServer(guild);

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Hey, thanks for adding me to your server.");
            embed.WithDescription("Hello! My name is " + Global.BotName + "!\n\n**__Links__**" +
                $"\n:computer: [Commands]({Global.websiteCommands})" +
                $"\n<:GitHub:529571722991763456> [Github Page]({Global.githubPage})" +
                $"\n:bookmark: [Documation]({Global.websiteHome})" +
                $"\n<:Discord:529572497130127360> [Creepysin Development Server]({Global.discordServers[1]})" +
                $"\n<:Discord:529572497130127360> [Creepysin Server]({Global.discordServers[0]})" +
                "\n\nIf you have any issues the best place to ask for assistance is on the Creepysin Server!");
            embed.WithColor(new Color(241, 196, 15));

            //Send a message to the server's default channel with the hello message
            await guild.DefaultChannel.SendMessageAsync("", false, embed.Build());

            //Send a message to Discord server's owner about setting up the bot
            var owner = await guild.Owner.GetOrCreateDMChannelAsync();
            await owner.SendMessageAsync($"Thanks for using {Global.BotName}! Check out {Global.websiteServerSetup} on how to setup {Global.BotName} for your server.");
        }

        private async Task UserLeft(SocketGuildUser user) //Says goodbye to the user.
        {
            var server = ServerLists.GetServer(user.Guild);
            if (!user.IsBot)
            {
                //Remove server data from account
                var account = UserAccounts.GetAccount(user);
                account.Servers.Remove(account.GetOrCreateServer(user.Guild.Id));
                UserAccounts.SaveAccounts();

                if (server.WelcomeMessageEnabled)
                {
                    var channel = _client.GetChannel(server.WelcomeChannel) as SocketTextChannel; //gets channel to send message in

                    string addUserMention = server.WelcomeGoodbyeMessage.Replace("[user]", user.Username);

                    await channel.SendMessageAsync(addUserMention); //Says goodbye.  
                }
            }
        }

        private async Task AnnounceJoinedUser(SocketGuildUser user) //welcomes New Players
        {
            Global.Log($"User {user} has joined the server {user.Guild.Name}({user.Guild.Id})");

            var server = ServerLists.GetServer(user.Guild);

            if (!user.IsBot)
            {
                //Pre create the user account
                UserAccounts.GetAccount(user);
                UserAccounts.SaveAccounts();
                if (server.WelcomeMessageEnabled)
                {
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
                    _isRunning = false;

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
                    _gameStatus = Console.ReadLine();

                    ActivityType activity = ActivityType.Playing;
                    string twich = null;
                    if (_isStreaming)
                    {
                        activity = ActivityType.Streaming;
                        twich = Config.bot.TwitchStreamingSite;
                    }  

                    await _client.SetGameAsync(_gameStatus, twich, activity);

                    Global.Log($"Bot's game was set to '{_gameStatus}'");
                }
                else if (input == "togglestream")
                {
                    if (_isStreaming)
                    {
                        _isStreaming = false;
                        await _client.SetGameAsync(_gameStatus, null, ActivityType.Playing);
                        Global.Log("Bot is no longer streaming");
                    }
                    else
                    {
                        _isStreaming = true;
                        await _client.SetGameAsync(_gameStatus, Config.bot.TwitchStreamingSite, ActivityType.Streaming);
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
                    Config.bot.IsAudioServiceEnabled = !Config.bot.IsAudioServiceEnabled;
                    Config.SaveConfig();

                    Global.Log($"The audio service was set to {Config.bot.IsAudioServiceEnabled}", ConsoleColor.Blue);
                    if (Config.bot.IsAudioServiceEnabled == true)
						AudioCheckService.CheckAudioService();
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

					AudioCheckService.UpdateAudioFiles();
                    Global.Log("Audio files were updated.", ConsoleColor.Blue);
                }
                else if(input == "status")
                {
                    Global.Log($"Bot status: {_client.ConnectionState.ToString()}\nServer count: {_client.Guilds.Count}\nLatency: {_client.Latency}");
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
            string token = Global.BotToken;
            string name = Global.BotName;
            string prefix = Global.BotPrefix;

            while (true)
            {
                string input = Console.ReadLine().Trim();

                if (input == "exit")
                {
                    if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(prefix))
                    {
                        Config.bot.BotToken = token;
                        Config.bot.BotName = name;
                        Config.bot.BotPrefix = prefix;

                        Config.SaveConfig();

                        Global.BotToken = token;
                        Global.BotName = name;
                        Global.BotPrefix = prefix;

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
            string giphyAPI = Config.bot.Apis.apiGiphyKey;
            string youtubeAPI = Config.bot.Apis.apiYoutubeKey;
            string googleAPI = Config.bot.Apis.apiGoogleSearchKey;
            string googleSearchID = Config.bot.Apis.googleSearchEngineID;

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
                    Config.bot.Apis.apiGiphyKey = giphyAPI;
                    Config.bot.Apis.apiYoutubeKey = youtubeAPI;
                    Config.bot.Apis.apiGoogleSearchKey = googleAPI;
                    Config.bot.Apis.googleSearchEngineID = googleSearchID;

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

            Console.WriteLine($"The current bot token is set to: '{Config.bot.BotToken}'");
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

            Console.WriteLine($"The current bot prefix is set to: '{Config.bot.BotPrefix}'");
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

            Console.WriteLine($"The current bot name is set to: '{Config.bot.BotName}'");
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

            Console.WriteLine($"The current bot Giphy key is set to: '{Config.bot.Apis.apiGiphyKey}'");
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

            Console.WriteLine($"The current bot Youtube key is set to: '{Config.bot.Apis.apiYoutubeKey}'");
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

            Console.WriteLine($"The current bot Google key is set to: '{Config.bot.Apis.apiGoogleSearchKey}'");
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

            Console.WriteLine($"The current bot Google Search ID is set to: '{Config.bot.Apis.googleSearchEngineID}'");
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
