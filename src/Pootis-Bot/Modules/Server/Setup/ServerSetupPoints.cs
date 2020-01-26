using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Preconditions;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupPoints : ModuleBase<SocketCommandContext>
	{
		[Command("setup set points")]
		[Summary("Sets the amount of points given")]
		[RequireGuildOwner]
		public async Task SetServerPoints(uint amount)
		{
			if (amount < 10)
			{
				await Context.Channel.SendMessageAsync("The points amount cannot be less then 10!");
				return;
			}

			ServerListsManager.GetServer(Context.Guild).PointGiveAmount = amount;
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The amount of points given out will now be {amount}.");
		}
	
		[Command("setup set pointscooldown")]
		[Summary("Changes the cooldown between when points are given out")]
		[RequireGuildOwner]
		public async Task SetServerPointsCooldown(int time)
		{
			if(time < 5)
			{
				await Context.Channel.SendMessageAsync("The time cannot be less then 5 seconds!");
				return;
			}

			ServerListsManager.GetServer(Context.Guild).PointsGiveCooldownTime = time;
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The cooldown time is now {time} seconds.");
		}
	}
}