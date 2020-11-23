using Newtonsoft.Json;
using Pootis_Bot.Config;
using Pootis_Bot.Console.ConfigMenus;

namespace Pootis_Bot.Core
{
	public class BotConfig : Config<BotConfig>
	{
		[MenuItemFormat("Token")] 
		[JsonProperty] 
		public string BotToken { get; internal set; }

		[MenuItemFormat("Prefix")]
		[JsonProperty] 
		public string BotPrefix { get; internal set; }
	}
}