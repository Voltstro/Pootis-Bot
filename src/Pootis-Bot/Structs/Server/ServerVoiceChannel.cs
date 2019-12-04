namespace Pootis_Bot.Structs.Server
{
	public struct ServerVoiceChannel
	{
		public ulong Id { get; set; }
		public string Name { get; set; }

		public ServerVoiceChannel(ulong id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}