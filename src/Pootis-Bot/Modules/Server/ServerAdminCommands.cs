using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Modules.Server
{
	public class ServerAdminCommands : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Commands for admins
		// Contributors     - Creepysin, 

		private const int MaxPurgeCount = 100;

		[Command("kick")]
		[Summary("Kicks a user")]
		[RequireBotPermission(GuildPermission.KickMembers)]
		[RequireUserPermission(GuildPermission.KickMembers)]
		public async Task KickUser(SocketGuildUser user, [Remainder] string reason = "")
		{
			if (user.GuildPermissions.Administrator)
			{
				await Context.Channel.SendMessageAsync(
					"Cannot kick this user due to them having Administrator rights!");
				return;
			}

			UserUtils.KickUser(user, (SocketGuildUser) Context.User, reason);
			await Context.Channel.SendMessageAsync($"The user {user.Username} was kicked.");
		}

		[Command("ban")]
		[Summary("Bans a user")]
		[RequireBotPermission(GuildPermission.BanMembers)]
		[RequireUserPermission(GuildPermission.BanMembers)]
		public async Task BanUser(SocketGuildUser user, [Remainder] string reason = "")
		{
			if (user.GuildPermissions.Administrator)
			{
				await Context.Channel.SendMessageAsync(
					"Cannot ban this user due to them having Administrator rights!");
				return;
			}

			UserUtils.BanUser(user, (SocketGuildUser) Context.User, reason);
			await Context.Channel.SendMessageAsync($"The user {user.Username} was banned.");
		}

		[Command("mute")]
		[Summary("Mutes a user")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		[RequireUserPermission(GuildPermission.ManageRoles)]
		public async Task MuteUser(SocketGuildUser user = null)
		{
			//Check to see if the user is null
			if (user == null)
			{
				await Context.Channel.SendMessageAsync("You need to input a username of a user!");
				return;
			}

			//Make sure the user being muted isn't the owner of the guild, because that would be retarded.
			if (user.Id == Context.Guild.OwnerId)
			{
				await Context.Channel.SendMessageAsync(
					"Excuse me, you are trying to mute... the owner? That is a terrible idea.");
				return;
			}

			//Yea, muting your self isn't normal either.
			if (user == Context.User)
			{
				await Context.Channel.SendMessageAsync(
					"Are you trying to mute your self? I don't think that is normal.");
				return;
			}

			UserAccount account = UserAccountsManager.GetAccount(user);
			UserAccountServerData accountServer = account.GetOrCreateServer(Context.Guild.Id);
			accountServer.IsMuted = true;

			UserAccountsManager.SaveAccounts();

			if (accountServer.IsMuted)
				await Context.Channel.SendMessageAsync($"**{user.Username}** is now muted.");
			else
				await Context.Channel.SendMessageAsync($"**{user.Username}** is now un-muted.");
		}

		[Command("purge", RunMode = RunMode.Async)]
		[Summary("Deletes bulk messages")]
		[RequireBotPermission(GuildPermission.ManageMessages)]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		public async Task Purge(int messageCount = 10)
		{
			if (messageCount > MaxPurgeCount)
			{
				await Context.Channel.SendMessageAsync($"You can only delete {MaxPurgeCount} messages at a time!");
				return;
			}

			try
			{
				IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(messageCount).FlattenAsync();
				await ((SocketTextChannel) Context.Channel).DeleteMessagesAsync(messages);
			}
			catch (ArgumentOutOfRangeException)
			{
				await Context.Channel.SendMessageAsync("There are not enough messages to delete in this channel with your specified message delete count.");
				return;
			}
			
			RestUserMessage message =
				await Context.Channel.SendMessageAsync(
					$"{messageCount} message were deleted, this message will be deleted in a moment.");
			await Task.Delay(3000);
			await message.DeleteAsync();
		}
	}
}