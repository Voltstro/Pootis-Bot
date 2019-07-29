namespace Pootis_Bot.Structs
{
	public struct VoiceChannel
	{
		public ulong ID { get; set; }
		public string Name { get; set; }

		public VoiceChannel(ulong id, string name)
		{
			ID = id;
			Name = name;
		}
	}
}
