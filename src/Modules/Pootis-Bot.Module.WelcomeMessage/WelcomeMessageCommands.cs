using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Helper;
using Pootis_Bot.Module.WelcomeMessage.Entities;

namespace Pootis_Bot.Module.WelcomeMessage
{
    [Group("welcomemessage")]
    [Name("Welcome/Goodbye Messages")]
    [Summary("Provides the commands for setting up welcome and goodbye messages")]
    [RequireBotPermission(GuildPermission.SendMessages)]
    public class WelcomeMessageCommands : ModuleBase<SocketCommandContext>
    {
        private readonly WelcomeMessageConfig config;
        
        public WelcomeMessageCommands()
        {
            config = Config<WelcomeMessageConfig>.Instance;
        }
        
        [Command("status")]
        [Summary("Status of the welcome and goodbye messages")]
        public async Task Status()
        {
            WelcomeMessageServer server = config.GetOrCreateWelcomeMessageServer(Context.Guild);
            
            SocketTextChannel channel = Context.Guild.GetTextChannel(server.ChannelId);

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Rule Reaction Status");
            embedBuilder.WithDescription($"Status of Rule Reaction for **{Context.Guild.Name}**");
            embedBuilder.AddField("Channel", channel == null ? "No Channel" : channel.Mention);
            embedBuilder.AddField("Welcome Message Enabled?", server.WelcomeMessageEnabled, true);
            embedBuilder.AddField("Welcome Message", server.WelcomeMessage, true);
            embedBuilder.AddField("Goodbye Message Enabled?", server.GoodbyeMessageEnabled, true);
            embedBuilder.AddField("Goodbye Message", server.GoodbyeMessage, true);
            await Context.Channel.SendEmbedAsync(embedBuilder);
        }
        
        [Command("channel")]
        [Summary("Sets the channel to where messages will be sent")]
        public async Task SetChannel([Remainder] SocketTextChannel channel)
        {
            config.GetOrCreateWelcomeMessageServer(Context.Guild).ChannelId = channel.Id;
            config.Save();

            await Context.Channel.SendMessageAsync($"Channel was set to {channel.Mention}.");
        }
        
        [Command("welcome")]
        [Summary("Sets the welcome message")]
        public async Task SetWelcomeMessage([Remainder] string message)
        {
            config.GetOrCreateWelcomeMessageServer(Context.Guild).WelcomeMessage = message;
            config.Save();

            await Context.Channel.SendMessageAsync("Welcome message set.");
        }

        [Command("goodbye")]
        [Summary("Sets the goodbye message")]
        public async Task SetGoodbyeMessage([Remainder] string message)
        {
            config.GetOrCreateWelcomeMessageServer(Context.Guild).GoodbyeMessage = message;
            config.Save();

            await Context.Channel.SendMessageAsync("Goodbye message set.");
        }

        [Command("enable welcome")]
        [Summary("Enables the welcome message")]
        public async Task EnableWelcomeMessage()
        {
            WelcomeMessageServer server = config.GetOrCreateWelcomeMessageServer(Context.Guild);

            //Check if its already enabled
            if (server.WelcomeMessageEnabled)
            {
                await Context.Channel.SendMessageAsync("The welcome message is already enabled!");
                return;
            }
            
            //Make sure we are ready to go
            if (!WelcomeMessageService.CheckServer(server, Context.Client) || string.IsNullOrWhiteSpace(server.WelcomeMessage))
            {
                await Context.Channel.SendMessageAsync("The welcome message is not setup correctly!");
                return;
            }

            //Enable it
            server.WelcomeMessageEnabled = true;
            config.Save();
            await Context.Channel.SendMessageAsync("The welcome message is now enabled.");
        }
        
        [Command("disable welcome")]
        [Summary("Disables the welcome message")]
        public async Task DisableWelcomeMessage()
        {
            WelcomeMessageServer server = config.GetOrCreateWelcomeMessageServer(Context.Guild);

            //Check if it is already disabled
            if (!server.WelcomeMessageEnabled)
            {
                await Context.Channel.SendMessageAsync("The welcome message is already disabled!");
                return;
            }

            //Disable it
            server.WelcomeMessageEnabled = false;
            config.Save();
            await Context.Channel.SendMessageAsync("The welcome message is now disabled.");
        }
        
        [Command("enable goodbye")]
        [Summary("Enables the goodbye message")]
        public async Task EnableGoodbyeMessage()
        {
            WelcomeMessageServer server = config.GetOrCreateWelcomeMessageServer(Context.Guild);

            //Check if its already enabled
            if (server.GoodbyeMessageEnabled)
            {
                await Context.Channel.SendMessageAsync("The goodbye message is already enabled!");
                return;
            }
            
            //Make sure we are ready to go
            if (!WelcomeMessageService.CheckServer(server, Context.Client) || string.IsNullOrWhiteSpace(server.GoodbyeMessage))
            {
                await Context.Channel.SendMessageAsync("The goodbye message is not setup correctly!");
                return;
            }

            //Enable it
            server.GoodbyeMessageEnabled = true;
            config.Save();
            await Context.Channel.SendMessageAsync("The goodbye message is now enabled.");
        }
        
        [Command("disable goodbye")]
        [Summary("Disables the goodbye message")]
        public async Task DisableGoodbyeMessage()
        {
            WelcomeMessageServer server = config.GetOrCreateWelcomeMessageServer(Context.Guild);

            //Check if it is already disabled
            if (!server.GoodbyeMessageEnabled)
            {
                await Context.Channel.SendMessageAsync("The goodbye message is already disabled!");
                return;
            }

            //Disable it
            server.GoodbyeMessageEnabled = false;
            config.Save();
            await Context.Channel.SendMessageAsync("The goodbye message is now disabled.");
        }
    }
}