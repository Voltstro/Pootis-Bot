using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Converters;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Modules.BotOwner
{
	public class BotCommands : ModuleBase<SocketCommandContext>
	{
		[Command("addxp")]
		[Summary("Adds xp to a specific user")]
		[RequireOwner]
		public async Task AddXp(SocketGuildUser user, uint amount)
		{
			LevelingSystem.UserSentMessage(user, (SocketTextChannel)Context.Channel, amount);

			await Task.Delay(500);

			await Context.Channel.SendMessageAsync(
				$"**{user.Username}** was given {amount} xp. They are now have {UserAccounts.GetAccount(user).Xp} xp in total.");
		}

		[Command("removexp")]
		[Summary("Removes xp from a specific user")]
		[RequireOwner]
		public async Task RemoveXp(SocketGuildUser user, uint amount)
		{
			LevelingSystem.UserSentMessage(user, (SocketTextChannel)Context.Channel, (uint)-amount);

			await Task.Delay(500);

			await Context.Channel.SendMessageAsync(
				$"**{user.Username}** had {amount} xp removed. They are now have {UserAccounts.GetAccount(user).Xp} xp in total.");
		}
	}
}
