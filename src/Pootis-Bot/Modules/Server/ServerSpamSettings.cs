using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Preconditions;

namespace Pootis_Bot.Modules.Server
{
	public class ServerSpamSettings : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Voltstro
		// Description      - Commands for spam settings
		// Contributors     - Voltstro, 

		[Command("spam toggle mentionuserspam")]
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

		[Command("spam set mentionuserthreshold")]
		[Summary("Set how much of a percentage of a servers users need to be mention before it is considered spam")]
		[RequireGuildOwner]
		public async Task SetMentionUserThreshold(int threshold)
		{
			ServerListsManager.GetServer(Context.Guild).AntiSpamSettings.MentionUsersPercentage = threshold;
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"The threshold for amount of users in a message was set to {threshold}.");
		}

		[Command("spam set roletorolewarnings")]
		[Summary("Sets how many role to role mention warnings before a proper warning will be given out")]
		[RequireGuildOwner]
		public async Task SetRoleToRoleMentionWarnings(int warnings)
		{
			ServerListsManager.GetServer(Context.Guild).AntiSpamSettings.RoleToRoleMentionWarnings = warnings;
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The amount of role to role warnings will be {warnings}.");
		}
	}
}