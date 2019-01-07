using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Pootis_Bot.Core
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _cmdService;

        string prefix;

        public async Task InitializeAsync(DiscordSocketClient client, string _prefix)
        {
            _client = client;
            _cmdService = new CommandService();
            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;

            prefix = _prefix;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;
            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            if (msg.Author.IsBot) //Check to see if user is bot, if is bot return.
                return;
            if (msg.HasStringPrefix(prefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _cmdService.ExecuteAsync(context, argPos);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Global.ColorMessage($"[{ Global.TimeNow()}] " + result.ErrorReason, ConsoleColor.Red);
                }
            }
        }

    }
}
