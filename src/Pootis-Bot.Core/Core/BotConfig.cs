using Pootis_Bot.Config;
using Pootis_Bot.Console;

namespace Pootis_Bot.Core
{
	public class BotConfig : Config<BotConfig>
	{
		[ConsoleConfigFormat("Token")] public string BotToken;
	}
}