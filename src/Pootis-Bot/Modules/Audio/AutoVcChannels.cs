using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services.Audio;

namespace Pootis_Bot.Modules.Audio
{
	public class AutoVcChannels : ModuleBase<ICommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Adds an auto voice channel
		// Contributors     - Creepysin, 

		[Command("addvcchannel")]
		[RequireBotPermission(GuildPermission.ManageChannels)]
		[RequireBotPermission(GuildPermission.MoveMembers)]
		[RequireUserPermission(GuildPermission.ManageChannels)]
		[Cooldown(5)]
		public async Task AddAutoVoiceChannel(string baseName)
		{
			await AutoVCChannelCreator.CreateAutoVCChannel((SocketGuild) Context.Guild, baseName);
			await Context.Channel.SendMessageAsync("Created auto VC channel.");
		}
	}
}