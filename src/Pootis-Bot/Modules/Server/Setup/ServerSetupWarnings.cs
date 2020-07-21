using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Preconditions;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupWarnings : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Voltstro
		// Description      - Commands related to setting up automatically getting kicked/banned on certain amount of warnings
		// Contributors     - Voltstro, 

		[Command("setup set warnskick")]
		[Summary("Sets how many warnings until a user gets kicked")]
		[RequireGuildOwner]
		public async Task SetWarnsKick(int warningsNeeded)
		{
			if (warningsNeeded < 1)
			{
				await Context.Channel.SendMessageAsync(
					"You need to set warnings for auto kick to be 1 or more warnings!");
				return;
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);
			server.WarningsKickAmount = warningsNeeded;

			//If the server's warnings for ban is lower then warnings for kick amount, then set it to one more then kick amount
			if (server.WarningsBanAmount < warningsNeeded)
				server.WarningsBanAmount = warningsNeeded + 1;

			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"{warningsNeeded} warnings will now be required for a user to be automatically kicked.");
		}

		[Command("setup set warnsban")]
		[Summary("Sets how many warnings until a user gets banned")]
		[RequireGuildOwner]
		public async Task SetWarnsBan(int warningsNeeded)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (warningsNeeded < server.WarningsKickAmount)
			{
				await Context.Channel.SendMessageAsync(
					$"You need to set warnings for auto ban to be {server.WarningsKickAmount} or more warnings!");
				return;
			}

			server.WarningsBanAmount = warningsNeeded;
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"{warningsNeeded} or more warnings will now be required for a user to be automatically banned.");
		}
	}
}