using System;

namespace Pootis_Bot.Console
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ConsoleConfigFormat : Attribute
	{
		public ConsoleConfigFormat(string formatted)
		{
			FormattedName = formatted;
		}

		internal readonly string FormattedName;
	}
}