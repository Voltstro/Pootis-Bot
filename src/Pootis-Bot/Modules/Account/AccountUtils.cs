using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Modules.Account
{
	public class AccountUtils : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Allows profile options
		// Contributors     - Creepysin, 

		[Command("profile")]
		[Summary("Gets your profile")]
		public async Task Profile()
		{
			IReadOnlyCollection<SocketRole> roles = ((SocketGuildUser) Context.User).Roles;
			List<SocketRole> sortedRoles = roles.OrderByDescending(o => o.Position).ToList();
			SocketRole userMainRole = sortedRoles.First();

			//Get the user's account and server data relating to the user
			UserAccount account = UserAccountsManager.GetAccount((SocketGuildUser) Context.User);
			UserAccountServerData accountServer = account.GetOrCreateServer(Context.Guild.Id);

			EmbedBuilder embed = new EmbedBuilder();

			string warningText = "No :sunglasses:";
			if (!accountServer.IsAccountNotWarnable && !((SocketGuildUser) Context.User).GuildPermissions.Administrator)
				warningText = $"Yes\n**Warnings: ** {accountServer.Warnings}";

			embed.WithCurrentTimestamp();
			embed.WithThumbnailUrl(Context.User.GetAvatarUrl());
			embed.WithTitle(Context.User.Username + "'s Profile");

			embed.AddField("Stats", $"**Level: ** {account.LevelNumber}\n**Xp: ** {account.Xp}\n", true);
			embed.AddField("Server",
				$"**Points: **{accountServer.Points}\n **Warnable: **{warningText}\n**Main Role: **{userMainRole.Name}\n",
				true);
			embed.AddField("Account", $"**Id: **{account.Id}\n**Creation Date: **{Context.User.CreatedAt}");

			embed.WithColor(userMainRole.Color);

			embed.WithFooter(account.ProfileMsg, Context.User.GetAvatarUrl());

			string description = "";

			if (Context.User.Id == Global.BotOwner.Id)
				description += $":crown: {Global.BotName} owner!\n";

			if (HighLevelProfileMessageManager.GetHighLevelProfileMessage(Context.User.Id) != null)
				description += HighLevelProfileMessageManager.GetHighLevelProfileMessage(Context.User.Id).Message;

			embed.WithDescription(description);

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		[Command("profile")]
		[Summary("Gets a person's profile")]
		public async Task Profile(SocketGuildUser user)
		{
			//Check to see if the user requested to view the profile of is a bot
			if (user.IsBot)
			{
				await Context.Channel.SendMessageAsync("You can not get a profile of a bot!");
				return;
			}

			//This will get the user's main role
			IReadOnlyCollection<SocketRole> roles = user.Roles;
			List<SocketRole> sortedRoles = roles.OrderByDescending(o => o.Position).ToList();
			SocketRole userMainRole = sortedRoles.First();

			//Get the user's account and server data relating to the user
			UserAccount account = UserAccountsManager.GetAccount(user);
			UserAccountServerData accountServer = account.GetOrCreateServer(Context.Guild.Id);
			EmbedBuilder embed = new EmbedBuilder();

			string warningText = "No :sunglasses:";
			if (!accountServer.IsAccountNotWarnable && !user.GuildPermissions.Administrator)
				warningText = $"Yes\n**Warnings: ** {accountServer.Warnings}";

			embed.WithCurrentTimestamp();
			embed.WithThumbnailUrl(user.GetAvatarUrl());
			embed.WithTitle(user.Username + "'s Profile");

			embed.AddField("Stats", $"**Level: ** {account.LevelNumber}\n**Xp: ** {account.Xp}\n", true);
			embed.AddField("Server",
				$"**Points: **{accountServer.Points}\n**Warnable: **{warningText}\n**Main Role: **{userMainRole.Name}\n",
				true);
			embed.AddField("Account", $"**Id: **{account.Id}\n**Creation Date: **{user.CreatedAt}");

			embed.WithColor(userMainRole.Color);

			embed.WithFooter(account.ProfileMsg, user.GetAvatarUrl());

			string description = "";

			if (user.Id == Global.BotOwner.Id)
				description += $":crown: {Global.BotName} owner!\n";

			if (HighLevelProfileMessageManager.GetHighLevelProfileMessage(user.Id) != null)
				description += HighLevelProfileMessageManager.GetHighLevelProfileMessage(user.Id).Message;

			embed.WithDescription(description);

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		[Command("profilemsg")]
		[Alias("profile msg", "profile message")]
		[Summary("Set your profile public message (This is on any Discord server with the same Pootis-Bot!)")]
		public async Task ProfileMsg([Remainder] string message = "")
		{
			UserAccount account = UserAccountsManager.GetAccount((SocketGuildUser) Context.User);
			account.ProfileMsg = message;
			UserAccountsManager.SaveAccounts();

			await Context.Channel.SendMessageAsync($"Your public profile message was set to '{message}'");
		}

		[Command("role")]
		[Alias("rolegive")]
		[Summary("Gives you a role (If requirements are met)")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task Role([Remainder] string roleGiveName)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			OptRole role = server.GetOptRole(roleGiveName);

			//Check to make sure the role give exists first
			if (role == null)
			{
				await Context.Channel.SendMessageAsync($"There is no role to give with the name of '{roleGiveName}'.");
				return;
			}

			//If there is a requirement check to make sure the user meets it first
			if (role.RoleRequiredId != 0)
				if (!((SocketGuildUser) Context.User).Roles.Contains(RoleUtils.GetGuildRole(Context.Guild,
					role.RoleRequiredId))
				)
				{
					await Context.Channel.SendMessageAsync("You do not meet the requirements to get this role!");
					return;
				}

			SocketRole roleToGive = RoleUtils.GetGuildRole(Context.Guild, role.RoleToGiveId);

			//See if the user already has the role
			if (((SocketGuildUser) Context.User).Roles.Contains(roleToGive))
			{
				await Context.Channel.SendMessageAsync("You already have this role!");
				return;
			}

			//Give the role
			await ((SocketGuildUser) Context.User).AddRoleAsync(roleToGive);
			await Context.Channel.SendMessageAsync($"You have been given the **{roleToGive.Name}** role.");
		}
	}
}