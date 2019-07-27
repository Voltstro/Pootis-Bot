using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Services;

namespace Pootis_Bot.Modules.Server
{
    public class ServerPermissions : ModuleBase<SocketCommandContext>
    {
        // Module Infomation
        // Orginal Author   - Creepysin
        // Description      - Anything permission related
        // Contributors     - Creepysin, 

        private readonly PermissionService _perm;

        public ServerPermissions(CommandService commandService)
        {
            _perm = new PermissionService(commandService);
        }

        [Command("perm")]
        [RequireOwner]
        public async Task Permission(string command, string subCmd, string role)
        {
            if (subCmd == "add")
                await _perm.AddPerm(command, role, Context.Channel, Context.Guild);
            else if (subCmd == "remove")
                await _perm.RemovePerm(command, role, Context.Channel, Context.Guild);
        }

        [Command("getbannedchannels")]
        [RequireOwner]
        public async Task GetBanedChannels()
        {
            var server = ServerLists.GetServer(Context.Guild);
            StringBuilder final = new StringBuilder();
            final.Append("**All banned channels**: \n");

            foreach(var channel in server.BanedChannels)
            {
                final.Append($"<#{channel}> (**ID**: {channel})\n");
            }

            await Context.Channel.SendMessageAsync(final.ToString());
        }

        [Command("addbanedchannel")]
        [RequireOwner]
        public async Task AddBanedChannel(SocketTextChannel channel)
        {
            ServerLists.GetServer(Context.Guild).GetOrCreateBanedChannel(channel.Id);
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Channel **{channel.Name}** has been added to the baned channels list for your server.");
        }

        [Command("removebanedchannel")]
        [RequireOwner]
        public async Task RemoveBanedChannel(SocketTextChannel channel)
        {
            ServerLists.GetServer(Context.Guild).BanedChannels.Remove(channel.Id);
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Channel **{channel.Name}** was removed from your server's baned channel list.");
        }
    }
}
