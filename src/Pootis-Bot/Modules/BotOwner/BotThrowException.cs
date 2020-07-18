using System;
using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;

namespace Pootis_Bot.Modules.BotOwner
{
	public class BotThrowException : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Voltstro
		// Description      - Makes the bot thrown an exception
		// Contributors     - Voltstro, 

		[Command("throwexception")]
		[Alias("throwexcept")]
		[Summary("Makes the bot throw an exception")]
		[RequireOwner]
#pragma warning disable 1998
		public async Task ThrowExcept([Remainder] string message = "Manually thrown exception")
#pragma warning restore 1998
		{
			Logger.Warn("Manually thrown exception at: {TimeNow}.", Global.TimeNow());
			throw new Exception(message);
		}
	}
}