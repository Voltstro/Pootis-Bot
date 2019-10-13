using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;

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
			UserAccount account = UserAccounts.GetAccount((SocketGuildUser) Context.User);
			UserAccountServerData accountServer = account.GetOrCreateServer(Context.Guild.Id);

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
			UserAccount account = UserAccounts.GetAccount(user);
			UserAccountServerData accountServer = account.GetOrCreateServer(Context.Guild.Id);
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
			UserAccount account = UserAccounts.GetAccount((SocketGuildUser) Context.User);
			account.ProfileMsg = message;
			UserAccounts.SaveAccounts();

			await Context.Channel.SendMessageAsync($"Your public profile message was set to '{message}'");
		}
	}
}