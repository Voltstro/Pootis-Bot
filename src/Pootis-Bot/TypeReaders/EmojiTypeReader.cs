using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.TypeReaders
{
	/// <summary>
	/// A <see cref="TypeReader"/> for an <see cref="Emoji"/>
	/// </summary>
	public class EmojiTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
			IServiceProvider services)
		{
			return Task.FromResult(input.ContainsOnlyOneEmoji() ? TypeReaderResult.FromSuccess(new Emoji(input)) : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Input was not an emoji!"));
		}
	}
}