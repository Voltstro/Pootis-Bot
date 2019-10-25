using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;

namespace Pootis_Bot.Modules.BotOwner
{
	public class BotCommands : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Bot owner commands
		// Contributors     - Creepysin, 

		[Command("addxp")]
		[Summary("Adds xp to a specific user")]
		[RequireOwner]
		public async Task AddXp(SocketGuildUser user, uint amount)
		{
			LevelingSystem.UserSentMessage(user, (SocketTextChannel)Context.Channel, amount);

			await Task.Delay(500);

			await Context.Channel.SendMessageAsync(
				$"**{user.Username}** was given {amount} xp. They are now have {UserAccountsManager.GetAccount(user).Xp} xp in total.");
		}

		[Command("removexp")]
		[Summary("Removes xp from a specific user")]
		[RequireOwner]
		public async Task RemoveXp(SocketGuildUser user, uint amount)
		{
			LevelingSystem.UserSentMessage(user, (SocketTextChannel)Context.Channel, (uint)-amount);

			await Task.Delay(500);

			await Context.Channel.SendMessageAsync(
				$"**{user.Username}** had {amount} xp removed. They are now have {UserAccountsManager.GetAccount(user).Xp} xp in total.");
		}

		[Command("leaveguild")]
		[Summary("Forces the bot to leave a guild")]
		[RequireOwner]
		public async Task LeaveGuild(ulong guildId)
		{
			SocketGuild guild = Context.Client.GetGuild(guildId);

			//Make sure the bot is in a guild with the provided guildId.
			if(guild != null)
				await guild.LeaveAsync();
			else
				await Context.Channel.SendMessageAsync($"The bot isn't in a guild with the id of {guildId}!");
		}

		[Command("guildlist")]
		[Alias("guilds", "servers", "allguilds")]
		[Summary("Sets a list of all the guilds the bot is in")]
		[RequireOwner]
		public async Task GuildList()
		{
			SocketGuild[] guilds = Context.Client.Guilds.ToArray();
			StringBuilder sb = new StringBuilder();
			sb.Append($"__**Guilds that {Global.BotName} is in**__\n```csharp\n");

			foreach (SocketGuild guild in guilds)
			{
				sb.Append($" # {guild.Name}\n  └ ID: {guild.Id}\n  └ Member Count: {guild.MemberCount}");
			}

			sb.Append("```");

			await Context.Channel.SendMessageAsync(sb.ToString());
		}
	}
}
