using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.TypeReaders
{
	/// <summary>
	/// The difference between Discord.NET's provided TimeSpanReader and this,
	/// is that this custom one removes whitespaces.
	/// <para>
	/// This makes it possible to do `4d 3h 2m` etc, instead of only being
	/// able to do `4d3h2m`
	/// </para>
	/// </summary>
	public class TimeSpanCustomReader : TypeReader
	{
		private static readonly string[] Formats =
		{
			"%d'd'%h'h'%m'm'%s's'", //4d3h2m1s
			"%d'd'%h'h'%m'm'", //4d3h2m
			"%d'd'%h'h'%s's'", //4d3h1s
			"%d'd'%h'h'", //4d3h
			"%d'd'%m'm'%s's'", //4d2m1s
			"%d'd'%m'm'", //4d2m
			"%d'd'%s's'", //4d1s
			"%d'd'", //4d
			"%h'h'%m'm'%s's'", //3h2m1s
			"%h'h'%m'm'", //3h2m
			"%h'h'%s's'", //3h1s
			"%h'h'", //3h
			"%m'm'%s's'", //2m1s
			"%m'm'", //2m
			"%s's'" //1s
		};

		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
			IServiceProvider services)
		{
			return TimeSpan.TryParseExact(input.ToLowerInvariant().RemoveWhitespace(), Formats,
				CultureInfo.InvariantCulture, out TimeSpan timeSpan)
				? Task.FromResult(TypeReaderResult.FromSuccess(timeSpan))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse TimeSpan"));
		}
	}
}