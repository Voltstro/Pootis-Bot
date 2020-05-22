using System;
using System.Collections.Generic;
using System.Linq;

namespace Pootis_Bot.Helpers
{
	/// <summary>
	/// Provides extensions for <see cref="string"/>
	/// </summary>
	public static class StringExtensions
	{
		private static readonly Dictionary<string, string> IllegalChars = new Dictionary<string, string>
		{
			["\\"] = " ",
			["/"] = " ",
			[":"] = "-",
			["*"] = " ",
			["?"] = "",
			["\""] = "'",
			["<"] = " ",
			[">"] = " ",
			["|"] = "-"
		};

		/// <summary>
		/// Removes whitespaces from a string
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string RemoveWhitespace(this string str)
		{
			return string.Join("",
				str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
		}

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

		/// <summary>
		/// Makes a <see cref="string"/> into a title.
		/// <para>So like this: Awesome Epic Title</para>
		/// </summary>
		/// <param name="title">The string to change</param>
		/// <returns></returns>
		public static string Title(this string title)
		{
			char[] array = title.ToCharArray();

			//Handle the first letter in the string.
			if (array.Length >= 1)
				if (char.IsLower(array[0]))
					array[0] = char.ToUpper(array[0]);

			//Scan through the letters, checking for spaces.
			// ... Uppercase the lowercase letters following spaces.
			for (int i = 1; i < array.Length; i++)
				if (array[i - 1] == ' ')
					if (char.IsLower(array[i]))
						array[i] = char.ToUpper(array[i]);

			return new string(array);
		}


		/// <summary>
		/// Removes not allowed characters that can't be in a windows file name
		/// </summary>
		/// <param name="input"></param>
		/// <returns>Formatted string</returns>
		public static string RemoveIllegalChars(this string input)
		{
			foreach ((string illegalChar, string replaceMent) in IllegalChars)
				input = input.Replace(illegalChar, replaceMent);

			return input;
		}
	}
}