using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Helpers;

namespace Pootis_Bot.Modules.Basic
{
	public class Utils : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Voltstro
		// Description      - General utils
		// Contributors     - Voltstro, 

		[Command("hasrole")]
		[Summary("Check if user has a role")]
		public async Task HasRole(string roleName, SocketGuildUser user)
		{
			//Get the role
			IRole role = RoleUtils.GetGuildRole(Context.Guild, roleName);
			if (role == null)
			{
				await Context.Channel.SendMessageAsync("That role doesn't exist!");
				return;
			}

			if (user.UserHaveRole(role.Id))
				await Context.Channel.SendMessageAsync($"**{user.Username}** has the role **{role.Name}**.");
			else
				await Context.Channel.SendMessageAsync($"**{user.Username}** doesn't have the role **{role}**.");
		}

		[Command("alluserroles")]
		[Summary("Gets all of a user's roles")]
		public async Task AllUserRoles(SocketGuildUser user)
		{
			IReadOnlyCollection<SocketRole> roles = user.Roles;
			StringBuilder allRoles = new StringBuilder();
			allRoles.Append($"{user.Username}'s roles: \n");

			List<SocketRole> sortedRoles = roles.OrderByDescending(o => o.Position).ToList();

			foreach (SocketRole role in sortedRoles)
			{
				string roleName = role.Name;
				if (role.Position == 0)
					roleName = "Default";

				allRoles.Append($"{roleName} | ");
			}

			await Context.Channel.SendMessageAsync(allRoles.ToString());
		}

		[Command("allroles")]
		[Summary("Gets all roles on the server")]
		public async Task GetAllRoles()
		{
			IReadOnlyCollection<IRole> roles = ((IGuildUser) Context.User).Guild.Roles;
			StringBuilder allRoles = new StringBuilder();

			allRoles.Append("All roles on this server: \n");

			List<IRole> sortedRoles = roles.OrderByDescending(o => o.Position).ToList();

			foreach (IRole role in sortedRoles)
			{
				string roleName = role.Name;
				if (role.Position == 0)
					roleName = "Default";

				allRoles.Append($"{roleName} | ");
			}

			await Context.Channel.SendMessageAsync(allRoles.ToString());
		}

		[Command("embedmessage")]
		[Alias("embed")]
		[Summary("Displays your message in an embed message")]
		public async Task CmdEmbedMessage(string title = "", [Remainder] string msg = "")
		{
			await Context.Channel.SendMessageAsync("", false, EmbedMessage(title, msg).Build());
		}

		[Command("ping")]
		[Summary("Ping Pong!")]
		public async Task Ping()
		{
			await Context.Channel.SendMessageAsync($"Pong! **{Context.Client.Latency}**ms");
		}

		#region Functions

		private EmbedBuilder EmbedMessage(string title, string msg)
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle(title);
			embed.WithDescription(msg);

			return embed;
		}

		#endregion
	}
}