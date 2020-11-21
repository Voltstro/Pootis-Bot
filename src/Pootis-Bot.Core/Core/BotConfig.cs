using Pootis_Bot.Config;
using Pootis_Bot.Console.ConfigMenus;

namespace Pootis_Bot.Core
{
	public class BotConfig : Config<BotConfig>
	{
		[MenuItemFormat("Token")] public string BotToken { get; internal set; }
		[MenuItemFormat("Prefix")] public string BotPrefix { get; internal set; }
	}
}