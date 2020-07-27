namespace Pootis_Bot.Structs.Server
{
	/// <summary>
	/// A server role point
	/// </summary>
	public struct ServerRolePoints
	{
		/// <summary>
		/// How many points are required
		/// </summary>
		public uint PointsRequired { get; set; }

		/// <summary>
		/// The role to add
		/// </summary>
		public ulong RoleId { get; set; }
	}
}