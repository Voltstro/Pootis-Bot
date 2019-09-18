namespace Pootis_Bot.Structs
{
	public struct VoiceChannel
	{
		public ulong Id { get; set; }
		public string Name { get; set; }

		public VoiceChannel(ulong id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}
