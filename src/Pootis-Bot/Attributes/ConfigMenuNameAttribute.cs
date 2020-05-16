using System;

namespace Pootis_Bot.Attributes
{
	public class ConfigMenuNameAttribute : Attribute
	{
		public ConfigMenuNameAttribute(string formattedNamed)
		{
			FormattedName = formattedNamed;
		}

		public string FormattedName { get; set; }
	}
}