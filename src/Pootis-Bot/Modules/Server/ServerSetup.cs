using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Modules.Server
{
	public class ServerSetup : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Helps the server owner set up the bot for use
		// Contributors     - Creepysin, 

		[Command("togglementionuser")]
		[Alias("toggle mention user")]
		[Summary("Enables / Disables the mention user anti-spam feature")]
		[RequireGuildOwner]
		public async Task ToggleMentionUserSpam()
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			server.AntiSpamSettings.MentionUserEnabled = !server.AntiSpamSettings.MentionUserEnabled;

			ServerListsManager.SaveServerList();
			await Context.Channel.SendMessageAsync(
				$"Mention user anti-spam was set to {server.AntiSpamSettings.MentionUserEnabled}.");
		}

		[Command("setmentionuserthreshold")]
		[Alias("set mention user threshold")]
		[Summary("Set how much of a percentage of a servers users need to be mention before it is considered spam")]
		[RequireGuildOwner]
		public async Task SetMentionUserThreshold(int threshold)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			server.AntiSpamSettings.MentionUsersPercentage = threshold;

			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"The threshold for amount of users in a message was set to {threshold}.");
		}

		[Command("addrolepoints")]
		[Alias("addpointsrole", "add points role", "add role points")]
		[Summary("Adds a role to give after a user gets a certain amount of points")]
		[RequireGuildOwner]
		public async Task AddRolePoints(uint pointsAmount, string roleName)
		{
			SocketRole role = RoleUtils.GetGuildRole(Context.Guild, roleName);
			if (role == null)
			{
				await Context.Channel.SendMessageAsync("That role doesn't exists!");
				return;
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (pointsAmount >= server.PointGiveAmount)
			{
				await Context.Channel.SendMessageAsync(
					$"Sorry, but you have to set the points require higher then {server.PointGiveAmount}");

				return;
			}

			ServerRolePoints serverRolePoints = new ServerRolePoints
			{
				RoleId = role.Id,
				PointsRequired = pointsAmount
			};

			server.ServerRolePoints.Add(serverRolePoints);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"Ok, when a user gets {pointsAmount} points they shell receive the **{role.Name}** role.\nPlease note any user who already have {pointsAmount} points won't get the role.");
		}

		[Command("removerolepoints")]
		[Alias("removepointsrole", "remove points role", "remove role points")]
		[Summary("Removes a role to give to a user after they get a certain amount of points")]
		[RequireGuildOwner]
		public async Task RemoveRolePoints(uint pointsAmount)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			ServerRolePoints serverRolePoints = server.GetServerRolePoints(pointsAmount);
			if (serverRolePoints.PointsRequired == 0)
			{
				await Context.Channel.SendMessageAsync(
					$"There are no role points that have {pointsAmount} points as there requirement!");
				return;
			}

			server.ServerRolePoints.Remove(serverRolePoints);
			ServerListsManager.SaveServerList();

			//TODO: Maybe a better reply?
			await Context.Channel.SendMessageAsync(
				$"The {serverRolePoints.PointsRequired} points required one was removed.");
		}

		[Command("rolepoints")]
		[Alias("pointsrole", "role points", "points role")]
		[Summary("Gets all the role points")]
		[RequireGuildOwner]
		public async Task RolePoints()
		{
			StringBuilder sb = new StringBuilder();
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			sb.Append("__**Server Role Points**__\n");

			foreach (ServerRolePoints rolePoint in server.ServerRolePoints)
				sb.Append(
					$"[{RoleUtils.GetGuildRole(Context.Guild, rolePoint.RoleId)}] **{rolePoint.PointsRequired}**\n");

			await Context.Channel.SendMessageAsync(sb.ToString());
		}

		[Command("rolegiveadd")]
		[Alias("role give add", "add role give")]
		[Summary("Assigns you a specified role if the user meets a requirement")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task RoleGiveAdd(string roleGiveName, string roleToGive, [Remainder] string roleRequired = "")
		{
			SocketRole roleToAssign = RoleUtils.GetGuildRole(Context.Guild, roleToGive);

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
				socketRoleRequired = RoleUtils.GetGuildRole(Context.Guild, roleRequired);
				if (socketRoleRequired == null)
				{
					await Context.Channel.SendMessageAsync($"Role {roleRequired} doesn't exist!");
					return;
				}
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);

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
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The role give was created with the name of **{roleGiveName}**.");
		}

		[Command("rolegiveremove")]
		[Alias("role give remove", "remove role give")]
		[Summary("Removes a role give")]
		public async Task RoleGiveRemove(string roleGiveName)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			RoleGive roleGive = server.GetRoleGive(roleGiveName);
			if (roleGive == null)
			{
				await Context.Channel.SendMessageAsync($"There is no role give with the name '{roleGiveName}''.");
				return;
			}

			server.RoleGives.Remove(roleGive);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"Removed role give '{roleGiveName}'.'");
		}
	}
}