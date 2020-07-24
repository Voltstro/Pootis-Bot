namespace Pootis_Bot.Entities
{
	/// <summary>
	/// A message that can appear on a user's profile
	/// </summary>
	public class HighLevelProfileMessage
	{
		/// <summary>
		/// The user ID that this message is for
		/// </summary>
		public ulong UserId { get; set; }

		/// <summary>
		/// The message
		/// </summary>
		public string Message { get; set; }
	}
}