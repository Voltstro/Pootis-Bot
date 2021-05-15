using Pootis_Bot.Config;

namespace Pootis_Bot.Module.Basic
{
    internal class GameStatusConfig : Config<GameStatusConfig>
    {
        public string DefaultMessage { get; set; } = "Pootis-Bot v2";
        public string StreamingUrl { get; set; } = "https://www.youtube.com/Voltstro";
    }
}