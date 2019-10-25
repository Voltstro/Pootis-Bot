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
			await UserAccountsManager.CheckUserWarnStatus((SocketGuildUser) user);
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
				UserAccount userAccount = UserAccountsManager.GetAccount(user);
				if (userAccount.GetOrCreateServer(Context.Guild.Id).IsAccountNotWarnable)
					builder.Append(user.Username + "\n");
			}

			await Context.Channel.SendMessageAsync(builder.ToString());
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
				UserAccountsManager.GetAccount(userGuild).GetOrCreateServer(userGuild.Guild.Id);

			if (userAccount.IsAccountNotWarnable) return $"**{userGuild}** is already not warnable.";

			userAccount.IsAccountNotWarnable = true;
			userAccount.Warnings = 0;
			UserAccountsManager.SaveAccounts();
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
				UserAccountsManager.GetAccount(userguild).GetOrCreateServer(userguild.Guild.Id);
			if (userAccount.IsAccountNotWarnable == false) return $"**{user}** is already warnable.";

			userAccount.IsAccountNotWarnable = false;
			UserAccountsManager.SaveAccounts();
			return $"**{user}** was made warnable.";
		}

		private string Warn(SocketUser user)
		{
			if (user.IsBot)
				return "You cannot give a warning to a bot!";

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
