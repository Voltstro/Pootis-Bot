namespace Pootis_Bot.Structs
{
	public struct CooldownInfo
	{
		public ulong UserId { get; }
		public int CommandHashCode { get; }

		public CooldownInfo(ulong userId, int commandHashCode)
		{
			UserId = userId;
			CommandHashCode = commandHashCode;
		}
	}
}