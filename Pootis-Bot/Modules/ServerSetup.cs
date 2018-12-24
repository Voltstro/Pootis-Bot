using Discord.Commands;
using Pootis_Bot.Core.ServerList;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pootis_Bot.Modules
{
    public class ServerSetup : ModuleBase<SocketCommandContext>
    {
        [Command("setupwelcomeid")]
        [RequireOwner]
        public async Task SetupWelcomeID(ulong ID)
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.welcomeID = ID;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync("Server welcome id was set to " + ID);
        }

        [Command("togglewelcome")]
        [RequireOwner]
        public async Task ToggleWelcome()
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.enableWelcome = server.enableWelcome = !server.enableWelcome;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync("Welcome users was set to " + server.enableWelcome);
        }

        [Command("setupadmin")]
        [RequireOwner]
        public async Task SetupAdmin(string adminRoleName)
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.adminRoleName = adminRoleName;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync("Admin role was set to " + adminRoleName);
        }

        [Command("addserver")]
        [RequireOwner]
        public async Task AddServer()
        {
            ServerLists.GetServer(Context.Guild);

            await Context.Channel.SendMessageAsync("Your server was added to the serverlist");
        }
    }
}
