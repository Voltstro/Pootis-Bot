using Pootis_Bot.Config;
using Pootis_Bot.Console.ConfigMenus;

namespace Pootis_Bot.Module.Basic;

public class GameStatusConfig : Config<GameStatusConfig>
{
    [MenuItemFormat("Default Message")]
    public string DefaultMessage { get; set; } = "Pootis-Bot v2";
    
    [MenuItemFormat("Streaming URL")]
    public string StreamingUrl { get; set; } = "https://www.youtube.com/Voltstro";
}