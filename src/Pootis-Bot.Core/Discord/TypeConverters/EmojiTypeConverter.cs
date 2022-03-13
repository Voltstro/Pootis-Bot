using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Pootis_Bot.Helper;

namespace Pootis_Bot.Discord.TypeConverters;

internal sealed class EmojiTypeConverter : TypeConverter<Emoji>
{
    public override ApplicationCommandOptionType GetDiscordType()
    {
        return ApplicationCommandOptionType.String;
    }

    public override Task<TypeConverterResult> ReadAsync(IInteractionContext context,
        IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        string input = option.Value as string;
        if (string.IsNullOrWhiteSpace(input))
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed,
                "Failed to parse emoji!"));

        input = input.ToLowerInvariant();
        //We got only ONE emoji
        MatchCollection matches = input.DoesContainEmoji();
        if (matches.Count == 1 && matches[0].Value == input)
            return Task.FromResult(TypeConverterResult.FromSuccess(new Emoji
            {
                Name = input
            }));
        //There are multiple emojis
        if (matches.Count != 0)
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed,
                "The emoji input contains multiple emojis!"));

        //Check if we are an Discord emote
        if (context.Guild is not SocketGuild guild)
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed,
                "Something went wrong trying to parse the emoji!"));

        //Get all emotes from the server
        GuildEmote emote = guild.Emotes.FirstOrDefault(x => $"<:{x.Name}:{x.Id}>" == input);
        if (emote == null)
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed,
                "The emoji is not valid!"));
        return Task.FromResult(TypeConverterResult.FromSuccess(new Emoji
        {
            Name = emote.Name,
            Id = emote.Id
        }));
    }
}