using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Modules.Server
{
	public class ServerAccountManagement : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Commands for server owners to manage user server data settings
		// Contributors     - Creepysin, 

		[Command("makenotwarnable")]
		[Alias("make not warnable")]
		[Summary("Makes the user not warnable")]
		public async Task NotWarnable([Remainder] IGuildUser user = null)
		{
			await Context.Channel.SendMessageAsync(MakeNotWarnable((SocketUser) user));
		}

		[Command("makewarnable")]
		[Alias("make warnable")]
		[Summary("Makes the user warnable")]
		public async Task MakeWarnable([Remainder] IGuildUser user = null)
		{
			await Context.Channel.SendMessageAsync(MakeWarnable((SocketUser) user));
		}

		[Command("warn")]
		[Summary("Warns the user")]
		[RequireBotPermission(GuildPermission.KickMembers)]
		[RequireBotPermission(GuildPermission.BanMembers)]
		public async Task WarnUser(IGuildUser user)
		{
			await Context.Channel.SendMessageAsync(Warn((SocketUser) user));
			await UserAccounts.CheckUserWarnStatus((SocketGuildUser) user);
		}

		[Command("getnotwarnable")]
		[Alias("get not warnable")]
		[Summary("Gets a list of people in the server who are not warnable")]
		public async Task GetNotWarnable()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("__**Users who are not warnable**__\n");

			foreach (SocketGuildUser user in Context.Guild.Users)
			{
				UserAccount userAccount = UserAccounts.GetAccount(user);
				if (userAccount.GetOrCreateServer(Context.Guild.Id).IsAccountNotWarnable)
					builder.Append(user.Username + "\n");
			}

			await Context.Channel.SendMessageAsync(builder.ToString());
		}

		[Command("rolegiveadd")]
		[Alias("role give add", "add role give")]
		[Summary("Assigns you a specified role if the user meets a requirement")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task RoleGiveAdd(string roleGiveName, string roleToGive, [Remainder] string roleRequired = "")
		{
			SocketRole roleToAssign = Global.GetGuildRole(Context.Guild, roleToGive);

			//Check to make sure the role exists first
			if (roleToAssign == null)
			{
				await Context.Channel.SendMessageAsync($"No role under the name '{roleToGive}' exists!");
				return;
			}

			SocketRole socketRoleRequired = null;

			//If a required role was specified, check to make sure it exists
			if (!string.IsNullOrWhiteSpace(roleRequired))
			{
				socketRoleRequired = Global.GetGuildRole(Context.Guild, roleRequired);
				if (socketRoleRequired == null)
				{
					await Context.Channel.SendMessageAsync($"Role {roleRequired} doesn't exist!");
					return;
				}
			}

			ServerList server = ServerLists.GetServer(Context.Guild);

			//Check to make sure a role give doesn't already exist first
			if (server.GetRoleGive(roleGiveName) != null)
			{
				await Context.Channel.SendMessageAsync($"A role give with the name '{roleGiveName}' already exist!");
				return;
			}

			RoleGive roleGive = new RoleGive
			{
				Name = roleGiveName,
				RoleToGiveId = roleToAssign.Id,
				RoleRequiredId = 0
			};

			if (socketRoleRequired != null)
				roleGive.RoleRequiredId = socketRoleRequired.Id;

			server.RoleGives.Add(roleGive);
			ServerLists.SaveServerList();

			await Context.Channel.SendMessageAsync($"The role give was created with the name of **{roleGiveName}**.");
		}

		[Command("rolegiveremove")]
		[Alias("role give remove", "remove role give")]
		[Summary("Removes a role give")]
		public async Task RoleGiveRemove(string roleGiveName)
		{
			ServerList server = ServerLists.GetServer(Context.Guild);
			RoleGive roleGive = server.GetRoleGive(roleGiveName);
			if (roleGive == null)
			{
				await Context.Channel.SendMessageAsync($"There is no role give with the name '{roleGiveName}''.");
				return;
			}

			server.RoleGives.Remove(roleGive);
			ServerLists.SaveServerList();

			await Context.Channel.SendMessageAsync($"Removed role give '{roleGiveName}'.'");
		}

		#region Functions

		private string MakeNotWarnable(SocketUser user)
		{
			if (user == null)
				return "That user doesn't exist!";

			if (user.IsBot)
				return "You can not change the warnable status of a bot!";

			SocketGuildUser userGuild = (SocketGuildUser) user;
			UserAccountServerData userAccount =
				UserAccounts.GetAccount(userGuild).GetOrCreateServer(userGuild.Guild.Id);

			if (userAccount.IsAccountNotWarnable) return $"**{userGuild}** is already not warnable.";

			userAccount.IsAccountNotWarnable = true;
			userAccount.Warnings = 0;
			UserAccounts.SaveAccounts();
			return $"**{userGuild}** was made not warnable.";
		}

		private string MakeWarnable(SocketUser user)
		{
			if (user == null)
				return "That user doesn't exist!";

			if (user.IsBot)
				return "You can not change the warnable status of a bot!";

			SocketGuildUser userguild = (SocketGuildUser) user;

			UserAccountServerData userAccount =
				UserAccounts.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);
			if (userAccount.IsAccountNotWarnable == false) return $"**{user}** is already warnable.";

			userAccount.IsAccountNotWarnable = false;
			UserAccounts.SaveAccounts();
			return $"**{user}** was made warnable.";
		}

		private string Warn(SocketUser user)
		{
			if (user.IsBot)
				return "You cannot give a warning to a bot!";

			SocketGuildUser userGuild = (SocketGuildUser) user;
			UserAccountServerData userAccount =
				UserAccounts.GetAccount(userGuild).GetOrCreateServer(userGuild.Guild.Id);

			if (userAccount.IsAccountNotWarnable)
				return $"A warning cannot be given to **{user}**. That person's account is set to not warnable.";

			userAccount.Warnings++;
			UserAccounts.SaveAccounts();
			return $"A warning was given to **{user}**";
		}

		#endregion
	}
}
