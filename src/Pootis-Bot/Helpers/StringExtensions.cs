using System;

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
	}
}
