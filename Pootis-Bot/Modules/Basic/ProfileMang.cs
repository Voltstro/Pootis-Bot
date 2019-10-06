using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Modules.Basic
{
	public class ProfileMang : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Handles anything to do with profile management
		// Contributors     - Creepysin, 

		[Command("makenotwarnable")]
		[Summary("Makes the user not warnable")]
		public async Task NotWarnable([Remainder] IGuildUser user = null)
		{
			await Context.Channel.SendMessageAsync(MakeNotWarnable((SocketUser) user));
		}

		[Command("makewarnable")]
		[Summary("Makes the user warnable.")]
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
		[Summary("Gets a list of people in the server who are not warnable")]
		public async Task GetNotWarnable()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("__**Users who are not warnable**__\n");

			foreach (SocketGuildUser user in Context.Guild.Users)
			{
				GlobalUserAccount userAccount = UserAccounts.GetAccount(user);
				if (userAccount.GetOrCreateServer(Context.Guild.Id).IsAccountNotWarnable)
					builder.Append(user.Username + "\n");
			}

			await Context.Channel.SendMessageAsync(builder.ToString());
		}

		[Command("profile")]
		[Summary("Gets your")]
		public async Task Profile()
		{
			//TODO: Test this quite well
			//This will get the user's main role
			IReadOnlyCollection<SocketRole> roles = ((SocketGuildUser) Context.User).Roles;
			List<SocketRole> sortedRoles = roles.OrderByDescending(o => o.Position).ToList();
			SocketRole userMainRole = sortedRoles.First();

			//Get the user's account and server data relating to the user
			GlobalUserAccount account = UserAccounts.GetAccount((SocketGuildUser) Context.User);
			GlobalUserAccount.GlobalUserAccountServer accountServer = account.GetOrCreateServer(Context.Guild.Id);

			EmbedBuilder embed = new EmbedBuilder();

			string warningText = "No :sunglasses:";
			if (!accountServer.IsAccountNotWarnable)
				warningText = $"Yes\n**Warnings: ** {accountServer.Warnings}";

			embed.WithCurrentTimestamp();
			embed.WithThumbnailUrl(Context.User.GetAvatarUrl());
			embed.WithTitle(Context.User.Username + "'s Profile");

			embed.AddField("Stats", $"**Level: ** {account.LevelNumber}\n**Xp: ** {account.Xp}\n", true);
			embed.AddField("Server", $"**Warnable: **{warningText}\n**Main Role: **{userMainRole.Name}\n", true);
			embed.AddField("Account", $"**Id: **{account.Id}\n**Creation Date: **{Context.User.CreatedAt}");

			embed.WithColor(userMainRole.Color);

			embed.WithFooter(account.ProfileMsg, Context.User.GetAvatarUrl());

			if (Context.User.Id == Global.BotOwner.Id)
				embed.WithDescription($":crown: {Global.BotName} owner!");

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
			GlobalUserAccount account = UserAccounts.GetAccount(user);
			GlobalUserAccount.GlobalUserAccountServer accountServer = account.GetOrCreateServer(Context.Guild.Id);
			EmbedBuilder embed = new EmbedBuilder();

			string warningText = "No :sunglasses:";
			if (!accountServer.IsAccountNotWarnable)
				warningText = $"Yes\n**Warnings: ** {accountServer.Warnings}";

			embed.WithCurrentTimestamp();
			embed.WithThumbnailUrl(user.GetAvatarUrl());
			embed.WithTitle(user.Username + "'s Profile");

			embed.AddField("Stats", $"**Level: ** {account.LevelNumber}\n**Xp: ** {account.Xp}\n", true);
			embed.AddField("Server", $"**Warnable: **{warningText}\n**Main Role: **{userMainRole.Name}\n", true);
			embed.AddField("Account", $"**Id: **{account.Id}\n**Creation Date: **{user.CreatedAt}");

			embed.WithColor(userMainRole.Color);

			embed.WithFooter(account.ProfileMsg, user.GetAvatarUrl());

			if (user.Id == Global.BotOwner.Id)
				embed.WithDescription($":crown: {Global.BotName} owner!");

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		[Command("profilemsg")]
		[Summary("Set your profile public message (This is on any Discord server with the same Pootis-Bot!)")]
		public async Task ProfileMsg([Remainder] string message = "")
		{
			GlobalUserAccount account = UserAccounts.GetAccount((SocketGuildUser) Context.User);
			account.ProfileMsg = message;
			UserAccounts.SaveAccounts();

			await Context.Channel.SendMessageAsync($"Your public profile message was set to '{message}'");
		}

		#region Functions

		private string MakeNotWarnable(SocketUser user)
		{
			if (user == null)
				return "That user doesn't exist!";

			if (user.IsBot)
				return "You can not change the warnable status of a bot!";

			SocketGuildUser userGuild = (SocketGuildUser) user;
			GlobalUserAccount.GlobalUserAccountServer userAccount =
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

			GlobalUserAccount.GlobalUserAccountServer userAccount =
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
			GlobalUserAccount.GlobalUserAccountServer userAccount =
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