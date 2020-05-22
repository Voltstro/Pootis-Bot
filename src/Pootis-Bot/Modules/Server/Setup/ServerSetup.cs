using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Preconditions;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetup : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Provides setup commands
		// Contributors     - Creepysin, 

		private readonly string[] _setupModules =
		{
			nameof(ServerSetupStatus), nameof(ServerSetupBannedChannels), nameof(ServerSetupOptRoles),
			nameof(ServerSetupPointRoles), nameof(ServerSetupPoints), nameof(ServerSetupRuleReaction),
			nameof(ServerSetupWelcomeGoodbyeMessage), nameof(ServerSetupWarnings)
		};

		[Command("setup")]
		[Summary("Provides basic help for server setup")]
		[RequireGuildOwner]
		public async Task ServerSetupHelp()
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("Server Setup Basic Help");

			StringBuilder info = new StringBuilder();
			info.Append(
				$"Here are all the commands related to setting up your Discord server with {Global.BotName}! For more information or help, read the setup docs [here]({Global.websiteServerSetup}).\n\n");

			foreach (string module in _setupModules)
			{
				ModuleInfo moduleInfo = DiscordModuleManager.GetModule(module);
				if (moduleInfo == null) continue;

				info.Append($"**{moduleInfo.Name}**\n");

				for (int i = 0; i < moduleInfo.Commands.Count; i++)
				{
					info.Append($"`{moduleInfo.Commands[i].Name}` ");
					if (i + 1 == moduleInfo.Commands.Count)
						info.Append("\n");
				}
			}

			embed.WithDescription(info.ToString());

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}
	}
}