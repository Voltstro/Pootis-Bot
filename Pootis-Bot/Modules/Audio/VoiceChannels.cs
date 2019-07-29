using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Modules.Audio
{
    public class VoiceChannels : ModuleBase<ICommandContext>
    {
        [Command("addvcchannel")]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.MoveMembers)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
		[Cooldown(5)]
        public async Task AddVoiceChannel(string baseName)
        {
            RestVoiceChannel channel = await (Context.Guild as SocketGuild).CreateVoiceChannelAsync($"➕ New {baseName} VC");

            await Context.Channel.SendMessageAsync($"Added {baseName} as an auto voice channel.");

			VoiceChannel voiceChannel = new VoiceChannel(channel.Id, baseName);

            ServerLists.GetServer((SocketGuild)Context.Guild).VoiceChannels.Add(voiceChannel);
            ServerLists.SaveServerList();
        }
    }
}
