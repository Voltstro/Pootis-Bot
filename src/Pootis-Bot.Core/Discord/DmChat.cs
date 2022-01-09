using System;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Discord;

/// <summary>
///     A DM chat with a user
/// </summary>
public class DmChat
{
    private readonly IDMChannel dm;
    
    /// <summary>
    ///     Creates a new <see cref="DmChat"/> instance
    /// </summary>
    /// <param name="user">The user to start a DM with</param>
    public DmChat(IUser user)
    {
        dm = user.CreateDMChannelAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Sends a message to a user
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="HttpException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<IUserMessage> SendMessage(string message)
    {
        try
        {
            return await dm.SendMessageAsync(message);
        }
        catch (HttpException)
        {
            Logger.Warn("An HttpException was thrown while trying to send a user a message, they might not allow DMs.");
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An unknown error was thrown while trying to send a user a message!");
            throw;
        }
    }
    
    /// <summary>
    ///     Sends a message to a user
    /// </summary>
    /// <param name="embed"></param>
    /// <returns></returns>
    /// <exception cref="HttpException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<IUserMessage> SendMessage(EmbedBuilder embed)
    {
        return await SendMessage(embed.Build());
    }
    
    /// <summary>
    ///     Sends a message to a user
    /// </summary>
    /// <param name="embed"></param>
    /// <returns></returns>
    /// <exception cref="HttpException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<IUserMessage> SendMessage(Embed embed)
    {
        try
        {
            return await dm.SendMessageAsync("", false, embed);
        }
        catch (HttpException)
        {
            Logger.Warn("An HttpException was thrown while trying to send a user a message, they might not allow DMs.");
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An unknown error was thrown while trying to send a user a message!");
            throw;
        }
    }
}