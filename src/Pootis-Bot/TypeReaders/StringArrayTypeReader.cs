using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Pootis_Bot.TypeReaders
{
	/// <summary>
	///		A <see cref="TypeReader"/> for parsing objects implementing <see cref="string"/> arrays.
	/// </summary>
	public class StringArrayTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			string[] result = input.Split(new[] {", ", ","}, StringSplitOptions.RemoveEmptyEntries);
			return Task.FromResult(TypeReaderResult.FromSuccess(result));
		}
	}
}
