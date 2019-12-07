using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
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
		public async Task NotWarnable([Remainder] SocketGuildUser[] users)
		{
			await Context.Channel.SendMessageAsync(MakeNotWarnable(users));
		}

		[Command("makewarnable")]
		[Alias("make warnable")]
		[Summary("Makes the user warnable")]
		public async Task Warnable([Remainder] SocketGuildUser[] users)
		{
			await Context.Channel.SendMessageAsync(MakeWarnable(users));
		}

		[Command("warn")]
		[Summary("Warns the user")]
		[RequireBotPermission(GuildPermission.KickMembers)]
		[RequireBotPermission(GuildPermission.BanMembers)]
		public async Task WarnUser([Remainder] SocketGuildUser user = null)
		{
			if (user == null)
			{
				await Context.Channel.SendMessageAsync("You need to input a valid username!");
				return;
			}

			await Context.Channel.SendMessageAsync(Warn(user));
			UserAccountsManager.CheckUserWarnStatus(user);
		}

		[Command("getnotwarnable")]
		[Alias("get not warnable", "notwarnable")]
		[Summary("Gets a list of people in the server who are not warnable")]
		public async Task GetNotWarnable()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("__**Users who are not warnable**__\n");

			foreach (SocketGuildUser user in Context.Guild.Users)
			{
				UserAccount userAccount = UserAccountsManager.GetAccount(user);
				if (userAccount.GetOrCreateServer(Context.Guild.Id).IsAccountNotWarnable)
					builder.Append(user.Username + "\n");
			}

			await Context.Channel.SendMessageAsync(builder.ToString());
		}

		#region Functions

		private static string MakeNotWarnable(IEnumerable<SocketGuildUser> users)
		{
			List<SocketGuildUser> usersToChange = new List<SocketGuildUser>();

			foreach (SocketGuildUser user in users)
			{
				if (user.IsBot)
					return "You cannot change the warnable status of a bot!";

				//if (user.GuildPermissions.Administrator)
				//	return "You cannot change the warnable status of an administrator!";

				if (UserAccountsManager.GetAccount(user).GetOrCreateServer(user.Guild.Id).IsAccountNotWarnable)
					return $"**{user.Username}** is already not warnable!";

				usersToChange.Add(user);
			}

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < usersToChange.Count; i++)
			{
				UserAccountServerData userAccount =
					UserAccountsManager.GetAccount(usersToChange[i]).GetOrCreateServer(usersToChange[i].Guild.Id);

				userAccount.IsAccountNotWarnable = true;
				userAccount.Warnings = 0;

				sb.Append(i + 1 == usersToChange.Count ? usersToChange[i].Username : $"{usersToChange[i].Username}, ");
			}

			UserAccountsManager.SaveAccounts();

			return sb.Length == 1 ? $"**{sb}** was made not warnable." : $"The accounts **{sb}** were all made not warnable.";
		}

		private static string MakeWarnable(IEnumerable<SocketGuildUser> users)
		{
			List<SocketGuildUser> usersToChange = new List<SocketGuildUser>();

			foreach (SocketGuildUser user in users)
			{
				if (user.IsBot)
					return "You cannot change the warnable status of a bot!";

				//if (user.GuildPermissions.Administrator)
				//	return "You cannot change the warnable status of an administrator!";

				if (!UserAccountsManager.GetAccount(user).GetOrCreateServer(user.Guild.Id).IsAccountNotWarnable)
					return $"**{user.Username}** is already warnable!";

				usersToChange.Add(user);
			}

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < usersToChange.Count; i++)
			{
				UserAccountServerData userAccount =
					UserAccountsManager.GetAccount(usersToChange[i]).GetOrCreateServer(usersToChange[i].Guild.Id);

				userAccount.IsAccountNotWarnable = false;

				sb.Append(i + 1 == usersToChange.Count ? usersToChange[i].Username : $"{usersToChange[i].Username}, ");
			}

			UserAccountsManager.SaveAccounts();

			return sb.Length == 1 ? $"**{sb}** was made warnable." : $"The accounts **{sb}** were all made warnable.";
		}

		private static string Warn(SocketUser user)
		{
			if (user.IsBot)
				return "You cannot give a warning to a bot!";

			if (((SocketGuildUser) user).GuildPermissions.Administrator)
				return "You cannot warn an administrator!";

			SocketGuildUser userGuild = (SocketGuildUser) user;
			UserAccountServerData userAccount =
				UserAccountsManager.GetAccount(userGuild).GetOrCreateServer(userGuild.Guild.Id);

			if (userAccount.IsAccountNotWarnable)
				return $"A warning cannot be given to **{user}**. That person's account is set to not warnable.";

			userAccount.Warnings++;
			UserAccountsManager.SaveAccounts();
			return $"A warning was given to **{user}**";
		}

		#endregion
	}
}