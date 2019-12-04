namespace Pootis_Bot.Structs.Server
{
	public struct ServerRoleToRoleMention
	{
		/// <summary>
		///     The base role
		/// </summary>
		public ulong RoleNotToMentionId { get; set; }

		/// <summary>
		///     The role that the base role isn't allowed to mention
		/// </summary>
		public ulong RoleId { get; set; }

		public ServerRoleToRoleMention(ulong roleNotToMention, ulong role)
		{
			RoleNotToMentionId = roleNotToMention;
			RoleId = role;
		}
	}
}