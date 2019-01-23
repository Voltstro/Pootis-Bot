using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using System.Threading.Tasks;

namespace Pootis_Bot.Modules
{
    public class ServerSetup : ModuleBase<SocketCommandContext>
    {
        [Command("setup")]
        [Summary("Displays setup info")]
        [RequireOwner]
        public async Task Setup()
        {
            var dm = await Context.User.GetOrCreateDMChannelAsync();
            var server = ServerLists.GetServer(Context.Guild);
            var embed = new EmbedBuilder();

            await Context.Channel.SendMessageAsync("Setup status was set to ur dms");

            embed.WithTitle("Setup");
            embed.WithColor(new Color(255, 81, 168));
            embed.WithDescription("<:Menu:537572055760109568> Here your setup status.\n\n");

            string welcometitle = "<:Cross:537572008574189578> Welcome Channel Disabled";                           // Welcome Message and channel
            string welocmedes = "Welcome channel is disabled\n";
            if(server.EnableWelcome == true && server.WelcomeID != 0)
            {
                welcometitle = "<:Check:537572054266806292> Welcome Channel Enabled";
                welocmedes = $"Welcome channel is enabled and is set to channel: {server.WelcomeID}\n";
            }
            embed.AddField(welcometitle, welocmedes);

            string rulestitle = "<:Cross:537572008574189578> Rules Message Disabled";                             // Rules message
            string rulesdes = "Rules message is disabled\n";
            if(server.IsRules == true && !string.IsNullOrWhiteSpace(server.RulesMessage))
            {
                rulestitle = "<:Check:537572054266806292> Rules Message Enabled";
                welocmedes = $"The rules message is enabled and is set to {server.RulesMessage}\n";
            }
            embed.AddField(rulestitle, rulesdes);

            string admintitle = "Admin Role Name";              // Admin role
            string admindes = $"Admin role name is set to: {server.AdminRoleName}\n";
            embed.AddField(admintitle, admindes);

            string stafftitle = "Staff Role Name";              // Staff role
            string staffdes = $"Staff role name is set to: {server.StaffRoleName}\n";
            embed.AddField(stafftitle, staffdes);

            await dm.SendMessageAsync("", false, embed.Build());
        }           

        [Command("setupwelcomeid")]
        [Summary("Sets the welcome id")]
        [RequireOwner]
        public async Task SetupWelcomeID(ulong ID)
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.WelcomeID = ID;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync("Server welcome id was set to " + ID);
        }

        [Command("togglewelcome")]
        [Summary("Enables or disables welcome")]
        [RequireOwner]
        public async Task ToggleWelcome()
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.EnableWelcome = server.EnableWelcome = !server.EnableWelcome;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync("Welcome users was set to " + server.EnableWelcome);
        }

        [Command("togglerules")]
        [Summary("Displays whether it should metion the rules in the welcome message")]
        [RequireOwner]
        public async Task ToggleRules()
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.EnableWelcome = server.IsRules = !server.IsRules;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync("Rules was set to " + server.IsRules);
        }

        [Command("rulesmsg")]
        [Summary("Set the rules message")]
        [RequireOwner]
        public async Task RulesMsg(string rulesmsg)
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.RulesMessage = rulesmsg;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"Rules message was set to '{rulesmsg}'");
        }

        [Command("setupadmin")]
        [Summary("Sets the admin role")]
        [RequireOwner]
        public async Task SetupAdmin(string adminRoleName)
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.AdminRoleName = adminRoleName;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync("Admin role was set to " + adminRoleName);
        }

        [Command("setupstaff")]
        [Summary("Sets the staff role")]
        [RequireOwner]
        public async Task SetupStaff(string staffRoleName)
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.StaffRoleName = staffRoleName;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync("Staff role was set to " + staffRoleName);
        }
    }
}
