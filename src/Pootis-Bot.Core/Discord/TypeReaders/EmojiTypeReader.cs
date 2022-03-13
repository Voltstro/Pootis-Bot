using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Helper;

namespace Pootis_Bot.Discord.TypeReaders;

/// <summary>
///     <see cref="TypeReader" /> for <see cref="Emoji" />
/// </summary>
internal class EmojiTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        //Is this an emoji or a Discord emote

        //We got only ONE emoji
        MatchCollection matches = input.DoesContainEmoji();
        if (matches.Count == 1 && matches[0].Value == input)
            return Task.FromResult(TypeReaderResult.FromSuccess(new Emoji
            {
                Name = input
            }));
        //There are multiple emojis
        if (matches.Count != 0)
            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                "The emoji input contains multiple emojis!"));

        //Check if we are an Discord emote
        if (context.Guild is not SocketGuild guild)
            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                "Something went wrong trying to parse the emoji!"));

        //Get all emotes from the server
        GuildEmote emote = guild.Emotes.FirstOrDefault(x => $"<:{x.Name}:{x.Id}>" == input);
        if (emote == null)
            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                "The emoji is not valid!"));
        return Task.FromResult(TypeReaderResult.FromSuccess(new Emoji
        {
            Name = emote.Name,
            Id = emote.Id
        }));
    }
}