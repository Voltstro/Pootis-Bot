using Discord;

namespace Pootis_Bot.Helper;

public static class MessageUtils
{
    private const string ChannelUrl = "https://discordapp.com/channels/{0}/{1}/{2}";
    
    /// <summary>
    ///     Gets a message URL
    /// </summary>
    /// <param name="message"></param>
    /// <param name="guild"></param>
    /// <returns></returns>
    public static string GetMessageUrl(this IMessage message, IGuild? guild = null)
    {
        return string.Format(ChannelUrl, guild != null ? guild.Id.ToString() : "@me", message.Channel.Id.ToString(),
            message.Id.ToString());
    }
}