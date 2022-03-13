using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Helper;
using Pootis_Bot.Logging;
using Pootis_Bot.Module.RuleReaction.Entities;

namespace Pootis_Bot.Module.RuleReaction;

/// <summary>
///     Provides stuff for the rule reaction service
/// </summary>
internal static class RuleReactionService
{
    private static readonly RuleReactionConfig Config;

    static RuleReactionService()
    {
        Config ??= Config<RuleReactionConfig>.Instance;
    }

    /// <summary>
    ///     Checks all servers
    /// </summary>
    /// <param name="client"></param>
    public static async Task CheckAllServer(DiscordSocketClient client)
    {
        Logger.Debug("Checking rule reaction servers...");

        foreach (RuleReactionServer server in Config.RuleReactionServers) await CheckServer(server, client, true);
    }

    /// <summary>
    ///     Checks a <see cref="RuleReactionServer" />
    /// </summary>
    /// <param name="server"></param>
    /// <param name="client"></param>
    /// <param name="careAboutEnabled"></param>
    /// <returns></returns>
    public static async Task<bool> CheckServer(RuleReactionServer server, DiscordSocketClient client,
        bool careAboutEnabled = false)
    {
        //Get guild
        SocketGuild guild = client.GetGuild(server.GuildId);
        if (guild == null)
        {
            Config.RuleReactionServers.Remove(server);
            Config.Save();

            Logger.Debug("Removed server {ServerId} from rule reaction as it no longer exists.", server.GuildId);
            return false;
        }

        if (careAboutEnabled)
            if (!server.Enabled)
                return true;

        //Check the role
        SocketRole role = guild.GetRole(server.RoleId);
        if (role == null)
        {
            DisableRole(server);
            return false;
        }

        //Check that the channel still exists
        SocketTextChannel channel = guild.GetTextChannel(server.ChannelId);
        if (channel == null)
        {
            DisableChannel(server);
            return false;
        }

        //Check the message still exists
        IMessage message = await channel.GetMessageAsync(server.MessageId);
        if (message != null) return true;

        DisableMessage(server);
        return false;
    }

    /// <summary>
    ///     Call when a reaction is added
    /// </summary>
    /// <param name="reaction"></param>
    /// <param name="client"></param>
    public static async Task ReactionAdded(SocketReaction reaction, DiscordSocketClient client)
    {
        RuleReactionServer? server = Config.RuleReactionServers.FirstOrDefault(x => x.MessageId == reaction.MessageId);
        if (server is not {Enabled: true})
            return;

        //If the emote is right, add the role
        if (reaction.Emote.Name == server.Emoji)
        {
            SocketGuildUser user = client.GetGuild(server.GuildId).GetUser(reaction.UserId);
            if (user.HasRole(server.RoleId))
                return;

            await user.AddRoleAsync(server.RoleId);
        }
    }

    /// <summary>
    ///     Call when a role is deleted
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public static Task RoleDeleted(SocketRole role)
    {
        RuleReactionServer server = Config.GetOrCreateRuleReactionServer(role.Guild.Id);
        if (server is not {Enabled: true})
            return Task.CompletedTask;

        if (server.RoleId == role.Id)
            DisableRole(server);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Call when a channel is deleted
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public static Task ChannelDeleted(SocketChannel channel)
    {
        RuleReactionServer? server = Config.RuleReactionServers.FirstOrDefault(x => x.ChannelId == channel.Id);
        if (server is not {Enabled: true})
            return Task.CompletedTask;

        if (server.ChannelId == channel.Id)
            DisableChannel(server);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Call when a message is deleted
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="messageChannel"></param>
    /// <returns></returns>
    public static Task MessageDeleted(Cacheable<IMessage, ulong> cache,
        Cacheable<IMessageChannel, ulong> messageChannel)
    {
        if (!cache.HasValue)
            return Task.CompletedTask;
        IMessage message = cache.Value;

        RuleReactionServer? server = Config.RuleReactionServers.FirstOrDefault(x => x.MessageId == message.Id);
        if (server is not {Enabled: true})
            return Task.CompletedTask;

        if (server.MessageId == message.Id)
            DisableMessage(server);
        return Task.CompletedTask;
    }

    private static void DisableRole(RuleReactionServer server)
    {
        server.RoleId = 0;
        server.Enabled = false;
        Config.Save();
        Logger.Debug("Disabled server {ServerId} rule reaction as the role no longer exists.", server.GuildId);
    }

    private static void DisableChannel(RuleReactionServer server)
    {
        server.Enabled = false;
        server.ChannelId = 0;
        server.MessageId = 0;
        Config.Save();

        Logger.Debug("Disabled server {ServerId} rule reaction as the text channel no longer exists.", server.GuildId);
    }

    private static void DisableMessage(RuleReactionServer server)
    {
        server.Enabled = false;
        server.MessageId = 0;
        Config.Save();

        Logger.Debug("Disabled server {ServerId} rule reaction as the message no longer exists.", server.GuildId);
    }
}