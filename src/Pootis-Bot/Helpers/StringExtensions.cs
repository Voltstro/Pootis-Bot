using System;
using System.Linq;

namespace Pootis_Bot.Helpers
{
	/// <summary>
	/// Provides extensions for <see cref="string"/>
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Removes whitespaces from a string
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string RemoveWhitespace(this string str) => string.Join("",
			str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

		/// <summary>
		/// Checks if a string contains unicode characters
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static bool ContainsUnicodeCharacter(this string input)
		{
			//TODO: Find a better way of checking if a string is an emoji only
			const int maxAnsiCode = 255;

			return input.Any(c => c > maxAnsiCode);
		}
	}
}
