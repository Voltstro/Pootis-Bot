using System.Collections.Generic;

namespace Pootis_Bot.Entities
{
	/// <summary>
	/// A help module
	/// </summary>
	public class HelpModule
	{
		/// <summary>
		/// The group this help module is called
		/// </summary>
		public string Group { get; set; }

		/// <summary>
		/// Add the Discord modules that are apart of this help module
		/// </summary>
		public List<string> Modules { get; set; }
	}
}