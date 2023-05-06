using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Helper;
using Pootis_Bot.Logging;
using Pootis_Bot.Module.RuleReaction.Entities;
using Emoji = Pootis_Bot.Discord.Emoji;

namespace Pootis_Bot.Module.RuleReaction;

[Group("rulereaction", "Provides rule reaction configuration commands")]
[RequireBotPermission(GuildPermission.ManageRoles)]
public class RuleReactionInteractions : InteractionModuleBase<SocketInteractionContext>
{
    private readonly RuleReactionConfig config;

    public RuleReactionInteractions()
    {
        config = Config<RuleReactionConfig>.Instance;
    }

    [SlashCommand("status", "Status of the rule reaction")]
    public async Task Status()
    {
        RuleReactionServer server = config.GetOrCreateRuleReactionServer(Context.Guild.Id);

        IMessage? message = null;
        SocketTextChannel? channel = Context.Guild.GetTextChannel(server.ChannelId);
        if (channel != null)
            message = await channel.GetMessageAsync(server.MessageId);
        
        //Role
        SocketRole? role = null;
        if(server.RoleId != 0)
            role = Context.Guild.GetRole(server.RoleId);

        EmbedBuilder embedBuilder = new();
        embedBuilder.WithTitle("Rule Reaction Status");
        embedBuilder.WithDescription($"Status of Rule Reaction for **{Context.Guild.Name}**");
        embedBuilder.AddField("Enabled?", server.Enabled);
        embedBuilder.AddField("Message", message == null ? "No Message Set" : $"[Link]({message.GetMessageUrl()})");
        embedBuilder.AddField("Emoji", string.IsNullOrEmpty(server.Emoji) ? "No Emoji Set" : server.Emoji);
        embedBuilder.AddField("Role", role == null ? "No Role Set" : role.Name);
        
        await RespondAsync(embed: embedBuilder.Build());
    }

    [SlashCommand("emoji", "Sets the emoji to use for reactions")]
    public async Task SetEmoji(Emoji emoji)
    {
        config.GetOrCreateRuleReactionServer(Context.Guild.Id).Emoji = emoji.Name;
        config.Save();
        await RespondAsync($"Rule reaction emoji was set to {emoji}.");
    }

    [SlashCommand("message", "Sets what message to use")]
    public async Task SetMessageCommand(string messageId, IMessageChannel? channel = null)
    {
        bool parsed = ulong.TryParse(messageId, out ulong result);
        if (!parsed)
        {
            await RespondAsync($"Message ID needs to be a number!");
            return;
        }
        
        IMessageChannel? channelThatItIsIn = channel;
        if (channelThatItIsIn == null)
            channelThatItIsIn = Context.Channel;

        await SetMessage(channelThatItIsIn, result, Context.Guild);
    }

    [SlashCommand("role", "Sets what role will be given on reaction")]
    public async Task SetRole(SocketRole role)
    {
        config.GetOrCreateRuleReactionServer(Context.Guild.Id).RoleId = role.Id;
        config.Save();

        await RespondAsync($"Rule reaction role was set to {role.Name}.");
    }

    [SlashCommand("enable", "Verify that everything is ready to go and enables rule reaction")]
    public async Task EnableRuleReaction()
    {
        RuleReactionServer server = config.GetOrCreateRuleReactionServer(Context.Guild.Id);
        if (server.Enabled)
        {
            await RespondAsync("Rule reaction is already enabled!");
            return;
        }

        if (!await RuleReactionService.CheckServer(server, Context.Client))
        {
            await RespondAsync("The rule reaction is not setup correctly!");
            return;
        }

        server.Enabled = true;
        config.Save();
        await RespondAsync("Rule reaction is now enabled.");
    }

    [SlashCommand("disable", "Disables rule reaction")]
    public async Task DisableRuleReaction()
    {
        RuleReactionServer server = config.GetOrCreateRuleReactionServer(Context.Guild.Id);
        if (!server.Enabled)
        {
            await RespondAsync("Rule reaction is already disabled!");
            return;
        }

        server.Enabled = false;
        config.Save();
        await RespondAsync("Rule reaction is now disabled.");
    }

    private async Task SetMessage(IMessageChannel channel, ulong messageId, SocketGuild guild)
    {
        IMessage message = await channel.GetMessageAsync(messageId);

        //That message doesn't exist
        if (message == null)
        {
            await RespondAsync("That message doesn't exist!");
            return;
        }

        //Set the message and channel IDs
        RuleReactionServer reactionServer = config.GetOrCreateRuleReactionServer(guild.Id);
        reactionServer.ChannelId = channel.Id;
        reactionServer.MessageId = message.Id;
        config.Save();

        Logger.Debug("Guild {GuildId} set their rule reaction message to {MessageId}.", guild.Id, message.Id);
        await RespondAsync("The rule reaction message is set!");
    }
}