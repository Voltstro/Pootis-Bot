using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Pootis_Bot.Modules.Server
{
	public class ServerAdminCommands : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author   - Creepysin
		// Description      - Commands for admins
		// Contributors     - Creepysin, 

		[Command("kick")]
		[Summary("Kicks a user")]
		[RequireBotPermission(GuildPermission.KickMembers)]
		[RequireUserPermission(GuildPermission.KickMembers)]
		public async Task KickUser(IGuildUser user, [Remainder] string reason = "")
		{
			await user.KickAsync(reason);
			await Context.Channel.SendMessageAsync($"The user {user.Username} was kicked");
		}

		[Command("ban")]
		[Summary("Bans a user")]
		[RequireBotPermission(GuildPermission.BanMembers)]
		[RequireUserPermission(GuildPermission.BanMembers)]
		public async Task BanUser(IGuildUser user, int days = 0, [Remainder] string reason = "")
		{
			await user.BanAsync(days, reason);
			await Context.Channel.SendMessageAsync($"The user {user.Username} was banned");
		}

		[Command("purge", RunMode = RunMode.Async)]
		[Summary("Deletes bulk messages")]
		[RequireBotPermission(GuildPermission.ManageMessages)]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		public async Task Purge(int messageCount = 10)
		{
			Task<IEnumerable<IMessage>> messages = Context.Channel.GetMessagesAsync(messageCount + 1).FlattenAsync();

			await ((SocketTextChannel) Context.Channel).DeleteMessagesAsync(messages.Result);

			RestUserMessage message =
				await Context.Channel.SendMessageAsync(
					$"{messageCount} message were deleted, this message will be deleted in a moment.");
			await Task.Delay(3000);
			await message.DeleteAsync();
		}
	}
}