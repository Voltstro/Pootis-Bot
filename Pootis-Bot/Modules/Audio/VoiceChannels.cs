using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using Pootis_Bot.Core;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Modules.Audio
{
    public class VoiceChannels : ModuleBase<ICommandContext>
    {
        [Command("addvcchannel")]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.MoveMembers)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task AddVoiceChannel(string baseName)
        {
            RestVoiceChannel channel = await (Context.Guild as SocketGuild).CreateVoiceChannelAsync($"➕ New {baseName} VC");

            await Context.Channel.SendMessageAsync($"Added {baseName} as an auto voice channel.");

            GlobalServerList.VoiceChannel voiceChannel = new GlobalServerList.VoiceChannel
            {
                ID = channel.Id,
                Name = baseName
            };

            ServerLists.GetServer((SocketGuild)Context.Guild).VoiceChannels.Add(voiceChannel);
            ServerLists.SaveServerList();
        }
    }
}
