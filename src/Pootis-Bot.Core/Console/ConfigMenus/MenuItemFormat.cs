using System;

namespace Pootis_Bot.Console.ConfigMenus
{
	/// <summary>
	///     Tells the <see cref="ConsoleConfigMenu{T}" /> how to format this property in the selection menu
	///     <para>If you don't use this the <see cref="ConsoleConfigMenu{T}" /> will just use the name of the property</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class MenuItemFormat : Attribute
	{
		internal readonly string FormattedName;

		public MenuItemFormat(string formatted)
		{
			FormattedName = formatted;
		}
	}
}