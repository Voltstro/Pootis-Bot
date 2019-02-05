using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules.Server
{
    public class SetPermissions : ModuleBase<SocketCommandContext>
    {
        [Command("permmakenotwarnable")]
        [RequireOwner]
        public async Task PermNotWarnable([Remainder]string role = "")
        {
            if (Global.CheckIfRoleExist(Context.Guild, role) == null)
            {
                await Context.Channel.SendMessageAsync("That role doesn't exist!");
                return;
            }

            var list = ServerLists.GetServer(Context.Guild);
            list.permissions.PermNotWarnableRole = role;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Command `makenotwarnable` permission was set to '{role}'");
        }

        [Command("permakewarnable")]
        [RequireOwner]
        public async Task PermWarnable([Remainder]string role = "")
        {
            if(Global.CheckIfRoleExist(Context.Guild, role) == null)
            {
                await Context.Channel.SendMessageAsync("That role doesn't exist!");
                return;
            }

            var list = ServerLists.GetServer(Context.Guild);
            list.permissions.PermMakeWarnableRole = role;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Command `makewarnable` permission was set to '{role}'");
        }

        [Command("permwarn")]
        [RequireOwner]
        public async Task PermWarn([Remainder]string role = "")
        {
            if (Global.CheckIfRoleExist(Context.Guild, role) == null)
            {
                await Context.Channel.SendMessageAsync("That role doesn't exist!");
                return;
            }

            var list = ServerLists.GetServer(Context.Guild);
            list.permissions.PermWarn = role;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Command `warn` permission was set to '{role}'");
        }

        [Command("permgoogle")]
        [RequireOwner]
        public async Task PermGoogle([Remainder]string role = "")
        {
            if (Global.CheckIfRoleExist(Context.Guild, role) == null)
            {
                await Context.Channel.SendMessageAsync("That role doesn't exist!");
                return;
            }

            var list = ServerLists.GetServer(Context.Guild);
            list.permissions.PermGoogle = role;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Command `google` permission was set to '{role}'");
        }

        [Command("permyoutube")]
        [RequireOwner]
        public async Task PermYoutube([Remainder]string role = "")
        {
            if (Global.CheckIfRoleExist(Context.Guild, role) == null)
            {
                await Context.Channel.SendMessageAsync("That role doesn't exist!");
                return;
            }

            var list = ServerLists.GetServer(Context.Guild);
            list.permissions.PermYT = role;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Command `youtube` permission was set to '{role}'");
        }

        [Command("permgiphy")]
        [RequireOwner]
        public async Task PermGiphy([Remainder]string role = "")
        {
            if (Global.CheckIfRoleExist(Context.Guild, role) == null)
            {
                await Context.Channel.SendMessageAsync("That role doesn't exist!");
                return;
            }

            var list = ServerLists.GetServer(Context.Guild);
            list.permissions.PermGiphy = role;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Command `giphy` permission was set to '{role}'");
        }

        [Command("permmusic")]
        [RequireOwner]
        public async Task PermMusic([Remainder]string role = "")
        {
            if (Global.CheckIfRoleExist(Context.Guild, role) == null)
            {
                await Context.Channel.SendMessageAsync("That role doesn't exist!");
                return;
            }

            var list = ServerLists.GetServer(Context.Guild);
            list.permissions.PermMusic = role;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Audio commands permission was set to '{role}'");
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
