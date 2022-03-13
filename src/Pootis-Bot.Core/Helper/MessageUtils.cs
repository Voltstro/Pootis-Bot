using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord;

namespace Pootis_Bot.Helper;

/// <summary>
///     Provides utils for messages
/// </summary>
public static class MessageUtils
{
    private const string ChannelUrl = "https://discordapp.com/channels/{0}/{1}/{2}";

    /// <summary>
    ///     Gets a message URL
    /// </summary>
    /// <param name="message"></param>
    /// <param name="guild"></param>
    /// <returns></returns>
    public static string GetMessageUrl(this IMessage message, [MaybeNull] IGuild guild = null)
    {
        return string.Format(ChannelUrl, guild != null ? guild.Id.ToString() : "@me", message.Channel.Id.ToString(),
            message.Id.ToString());
    }

    /// <summary>
    ///     Send an error message to a message channel
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<IUserMessage> SendErrorMessageAsync(this IMessageChannel channel,
        [DisallowNull] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message), "Message cannot be null or blank!");

        return await channel.SendMessageAsync($"❌ {message}");
    }

    /// <summary>
    ///     Sends an embed using <see cref="EmbedBuilder" />. The <see cref="Embed" /> will be built for you.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="embed"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<IUserMessage> SendEmbedAsync(this IMessageChannel channel,
        [DisallowNull] EmbedBuilder embed)
    {
        if (embed == null)
            throw new ArgumentNullException(nameof(embed));

        return await SendEmbedAsync(channel, embed.Build());
    }

    /// <summary>
    ///     Sends an embed using <see cref="Embed" />
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="embed"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<IUserMessage> SendEmbedAsync(this IMessageChannel channel, [DisallowNull] Embed embed)
    {
        if (embed == null)
            throw new ArgumentNullException(nameof(embed));

        return await channel.SendMessageAsync("", false, embed);
    }
}