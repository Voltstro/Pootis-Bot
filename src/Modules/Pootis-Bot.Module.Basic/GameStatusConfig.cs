using Pootis_Bot.Config;

namespace Pootis_Bot.Module.Basic;

public class GameStatusConfig : Config<GameStatusConfig>
{
    public string DefaultMessage { get; set; } = "Pootis-Bot v2";
    public string StreamingUrl { get; set; } = "https://www.youtube.com/Voltstro";
}