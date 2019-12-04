namespace Pootis_Bot.Entities
{
	public class RoleGive
	{
		public string Name { get; set; }

		public ulong RoleToGiveId { get; set; }
		public ulong RoleRequiredId { get; set; }
	}
}