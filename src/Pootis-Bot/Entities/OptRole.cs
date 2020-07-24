namespace Pootis_Bot.Entities
{
	/// <summary>
	/// A role that user's can opt into
	/// </summary>
	public class OptRole
	{
		/// <summary>
		/// The name of this opt role
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The ID of the role that this opt role will give
		/// </summary>
		public ulong RoleToGiveId { get; set; }

		/// <summary>
		/// An optional role ID that will be required to get this opt role
		/// </summary>
		public ulong RoleRequiredId { get; set; }
	}
}