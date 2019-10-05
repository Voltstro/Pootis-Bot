namespace Pootis_Bot.Structs
{
	public struct RoleToRoleMention
	{
		public ulong RoleNotToMentionId { get; set; }
		public ulong RoleId { get; set; }

		public RoleToRoleMention(ulong roleNotToMention, ulong role)
		{
			RoleNotToMentionId = roleNotToMention;
			RoleId = role;
		}
	}
}