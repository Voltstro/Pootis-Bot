using System;

namespace Pootis_Bot.Console.ConfigMenus
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MenuItemFormat : Attribute
	{
		public MenuItemFormat(string formatted)
		{
			FormattedName = formatted;
		}

		internal readonly string FormattedName;
	}
}