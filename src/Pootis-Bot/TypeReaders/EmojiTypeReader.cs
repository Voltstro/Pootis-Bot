using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Pootis_Bot.TypeReaders
{
	public class EmojiTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			return Task.FromResult(TypeReaderResult.FromSuccess(new Emoji(input)));
		}
	}
}
