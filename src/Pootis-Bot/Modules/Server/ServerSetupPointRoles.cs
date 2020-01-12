using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Modules.Server
{
	public class ServerSetupPointRoles : ModuleBase<SocketCommandContext>
	{
		[Command("setup add pointrole")]
		[Summary("Automatically assigns a user a role when they reach X amount of points")]
		[RequireGuildOwner]
		public async Task AddRolePoints(uint pointsAmount, [Remainder] string roleName)
		{
			//First, lets make sure that the role actually exists
			SocketRole role = RoleUtils.GetGuildRole(Context.Guild, roleName);
			if (role == null)
			{
				await Context.Channel.SendMessageAsync("That role doesn't exists!");
				return;
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);

			//Make sure a point role with that many points doesn't exist
			if (server.GetServerRolePoints(pointsAmount).PointsRequired != 0)
			{
				await Context.Channel.SendMessageAsync(
					$"A role is already given when a user gets `{pointsAmount}` points.");
				return;
			}

			//Can't have a point role lower then the amount of points given each time
			if (pointsAmount >= server.PointGiveAmount)
			{
				await Context.Channel.SendMessageAsync(
					$"You have to set required points higher then `{server.PointGiveAmount}`!");

				return;
			}

			ServerRolePoints serverRolePoints = new ServerRolePoints
			{
				RoleId = role.Id,
				PointsRequired = pointsAmount
			};

			//Add our point role to our list
			server.ServerRolePoints.Add(serverRolePoints);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"Ok, when a user gets {pointsAmount} points they shell receive the **{role.Name}** role.\nPlease note any user who already have {pointsAmount} points won't get the role.");
		}

		[Command("setup remove pointrole")]
		[Summary("Removes a point role")]
		[RequireGuildOwner]
		public async Task RemoveRolePoints(uint pointsAmount)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			ServerRolePoints pointRole = server.GetServerRolePoints(pointsAmount);
			if (pointRole.PointsRequired == 0) //Make sure a point role with that amount of points needed actually exists.
			{
				await Context.Channel.SendMessageAsync($"There are no point roles with the points amount of `{pointsAmount}`.");
				return;
			}

			server.ServerRolePoints.Remove(pointRole);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The point role with `{pointsAmount}` points was removed.");
		}

		
		[Command("setup pointroles")]
		[Summary("Gets all the role points")]
		[RequireGuildOwner]
		public async Task RolePoints()
		{
			StringBuilder sb = new StringBuilder();
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			sb.Append("__**Server Point Roles**__\n");

			foreach (ServerRolePoints rolePoint in server.ServerRolePoints)
				sb.Append(
					$"[{RoleUtils.GetGuildRole(Context.Guild, rolePoint.RoleId)}] **{rolePoint.PointsRequired}**\n");

			await Context.Channel.SendMessageAsync(sb.ToString());
		}
	}
}
