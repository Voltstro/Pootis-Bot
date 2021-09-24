namespace Pootis_Bot.Module.Upgrade.OldEntities.Server
{
    /// <summary>
    /// A audio voice channel
    /// </summary>
    public struct ServerAudioVoiceChannel
    {
        /// <summary>
        /// The VC channel's ID
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The name of this audio voice channel
        /// </summary>
        public string Name { get; set; }

        public ServerAudioVoiceChannel(ulong id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}