namespace Pootis_Bot.Structs
{
	/// <summary>
	/// Info for a command cooldown
	/// </summary>
	public struct CooldownInfo
	{
		/// <summary>
		/// The user ID that this cooldown applies to
		/// </summary>
		public ulong UserId { get; }

		/// <summary>
		/// The hash code for what command this applies to
		/// </summary>
		public int CommandHashCode { get; }

		public CooldownInfo(ulong userId, int commandHashCode)
		{
			UserId = userId;
			CommandHashCode = commandHashCode;
		}
	}
}