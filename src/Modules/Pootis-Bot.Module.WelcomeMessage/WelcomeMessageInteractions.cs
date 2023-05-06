using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Logging;
using Pootis_Bot.Module.WelcomeMessage.Entities;

namespace Pootis_Bot.Module.WelcomeMessage;

[RequireBotPermission(GuildPermission.SendMessages)]
[Group("welcomemessage", "Welcome and goodbye message configuration commands")]
public class WelcomeMessageInteractions : InteractionModuleBase<SocketInteractionContext>
{
    private readonly WelcomeMessageConfig config;

    public WelcomeMessageInteractions()
    {
        config = Config<WelcomeMessageConfig>.Instance;
    }

    [SlashCommand("status", "Gets the status of your welcome & goodbye messages configuration")]
    public async Task Status()
    {
        try
        {
            WelcomeMessageServer server = config.GetOrCreateWelcomeMessageServer(Context.Guild);

            SocketTextChannel channel = Context.Guild.GetTextChannel(server.ChannelId);

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithTitle("Welcome Message Status");
            embedBuilder.WithDescription($"Status of Welcome Message for **{Context.Guild.Name}**");
            embedBuilder.AddField("Channel", channel == null ? "No Channel" : channel.Mention);
            embedBuilder.AddField("Welcome Message Enabled?", server.WelcomeMessageEnabled, true);
            embedBuilder.AddField("Welcome Message", server.WelcomeMessage ?? "No welcome message set!", true);
            embedBuilder.AddField("Goodbye Message Enabled?", server.GoodbyeMessageEnabled, true);
            embedBuilder.AddField("Goodbye Message", server.GoodbyeMessage ?? "No goodbye message set!", true);
            await RespondAsync(embed: embedBuilder.Build());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An error occured in welcome message status!");
            throw;
        }
    }

    [SlashCommand("channel", "Sets the channel to where messages will be sent")]
    public async Task SetChannel(SocketTextChannel channel)
    {
        config.GetOrCreateWelcomeMessageServer(Context.Guild).ChannelId = channel.Id;
        config.Save();

        await RespondAsync($"Channel was set to {channel.Mention}.");
    }
    
    [SlashCommand("welcome", "Sets the welcome message")]
    public async Task SetWelcomeMessage(string message)
    {
        config.GetOrCreateWelcomeMessageServer(Context.Guild).WelcomeMessage = message;
        config.Save();

        await RespondAsync("Welcome message has been set.");
    }

    [SlashCommand("goodbye", "Sets the goodbye message")]
    public async Task SetGoodbyeMessage(string message)
    {
        config.GetOrCreateWelcomeMessageServer(Context.Guild).GoodbyeMessage = message;
        config.Save();

        await RespondAsync("Goodbye message has been set.");
    }

    [SlashCommand("enable-welcome", "Enables the welcome message")]
    public async Task EnableWelcomeMessage()
    {
        WelcomeMessageServer server = config.GetOrCreateWelcomeMessageServer(Context.Guild);

        //Check if its already enabled
        if (server.WelcomeMessageEnabled)
        {
            await RespondAsync("The welcome message is already enabled!");
            return;
        }

        //Make sure we are ready to go
        if (!WelcomeMessageService.CheckServer(server, Context.Client) ||
            string.IsNullOrWhiteSpace(server.WelcomeMessage))
        {
            await RespondAsync("The welcome message is not setup correctly!");
            return;
        }

        //Enable it
        server.WelcomeMessageEnabled = true;
        config.Save();
        await RespondAsync("The welcome message is now enabled.");
    }

    [SlashCommand("disable-welcome", "Disables the welcome message")]
    public async Task DisableWelcomeMessage()
    {
        WelcomeMessageServer server = config.GetOrCreateWelcomeMessageServer(Context.Guild);

        //Check if it is already disabled
        if (!server.WelcomeMessageEnabled)
        {
            await RespondAsync("The welcome message is already disabled!");
            return;
        }

        //Disable it
        server.WelcomeMessageEnabled = false;
        config.Save();
        await RespondAsync("The welcome message is now disabled.");
    }
    
    [SlashCommand("enable-goodbye", "Enables the goodbye message")]
    public async Task EnableGoodbyeMessage()
    {
        WelcomeMessageServer server = config.GetOrCreateWelcomeMessageServer(Context.Guild);

        //Check if its already enabled
        if (server.GoodbyeMessageEnabled)
        {
            await RespondAsync("The goodbye message is already enabled!");
            return;
        }

        //Make sure we are ready to go
        if (!WelcomeMessageService.CheckServer(server, Context.Client) ||
            string.IsNullOrWhiteSpace(server.GoodbyeMessage))
        {
            await RespondAsync("The goodbye message is not setup correctly!");
            return;
        }

        //Enable it
        server.GoodbyeMessageEnabled = true;
        config.Save();
        await RespondAsync("The goodbye message is now enabled.");
    }
    
    [SlashCommand("disable-goodbye", "Disables the goodbye message")]
    public async Task DisableGoodbyeMessage()
    {
        WelcomeMessageServer server = config.GetOrCreateWelcomeMessageServer(Context.Guild);

        //Check if it is already disabled
        if (!server.GoodbyeMessageEnabled)
        {
            await RespondAsync("The goodbye message is already disabled!");
            return;
        }

        //Disable it
        server.GoodbyeMessageEnabled = false;
        config.Save();
        await RespondAsync("The goodbye message is now disabled.");
    }
}