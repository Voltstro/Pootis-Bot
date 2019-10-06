using System;
using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules.BotOwner
{
	public class BotThrowException : ModuleBase<SocketCommandContext>
	{
		[Command("thorwexception")]
		[Alias("throwexcept")]
		[Summary("Makes the bot throw an exception")]
		[RequireOwner]
#pragma warning disable 1998
		public async Task ThrowExcept([Remainder] string message = "Manually thrown exception")
#pragma warning restore 1998
		{
			Global.Log($"Manually thrown exception at: {Global.TimeNow()}.", ConsoleColor.Yellow);
			throw new Exception(message);
		}

	}
}
