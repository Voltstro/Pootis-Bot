using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Services;

namespace Pootis_Bot.Modules.Server
{
    public class SetPermissions : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly PermissionService _perm;

        public SetPermissions(CommandService commandService)
        {
            _service = commandService;
            _perm = new PermissionService(_service);
        }

        [Command("perm")]
        [RequireOwner]
        public async Task Permission(string command, string role)
        {
            await _perm.SetPermission(command, role, Context.Channel, Context.Guild);
        }

        [Command("addbanedchannel")]
        [RequireOwner]
        public async Task AddBanedChannel(SocketTextChannel channel)
        {
            var server = ServerLists.GetServer(Context.Guild).GetOrCreateBanedChannel(channel.Id);

            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Channel {channel.Name} has been added to the baned channels for your server");
        }

        [Command("removebanedchannel")]
        [RequireOwner]
        public async Task RemoveBanedChannel(SocketTextChannel channel)
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.DeleteChannel(channel.Id);

            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Channel {channel.Name} was removed from your server's baned channel list");
        }
    }
}
