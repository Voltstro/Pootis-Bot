namespace Pootis_Bot.Structs
{
	public struct RoleToRoleMention
	{
		public string RoleNotToMention { get; set; }
		public string Role { get; set; }

		public RoleToRoleMention(string roleNotToMention, string role)
		{
			RoleNotToMention = roleNotToMention;
			Role = role;
		}
	}
}
