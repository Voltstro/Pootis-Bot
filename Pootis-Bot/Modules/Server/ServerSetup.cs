using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using System.Threading.Tasks;

namespace Pootis_Bot.Modules
{
    public class ServerSetup : ModuleBase<SocketCommandContext>
    {
        // Module Infomation
        // Orginal Author   - Creepysin
        // Description      - Helps the server owner set up the bot for use
        // Contributors     - Creepysin, 

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
            if(server.WelcomeMessageEnabled == true && server.WelcomeChannel != 0)
            {
                welcometitle = "<:Check:537572054266806292> Welcome Channel Enabled";
                welocmedes = $"Welcome channel is enabled and is set to channel: {server.WelcomeChannel}\n";
            }
            embed.AddField(welcometitle, welocmedes);

            string admintitle = "Admin Role Name";                                                                // Admin role
            string admindes = $"Admin role name is set to: {server.AdminRoleName}\n";
            embed.AddField(admintitle, admindes);

            string stafftitle = "Staff Role Name";                                                                // Staff role
            string staffdes = $"Staff role name is set to: {server.StaffRoleName}\n";
            embed.AddField(stafftitle, staffdes);

            await dm.SendMessageAsync("", false, embed.Build());
        }

        [Command("togglewelcomemessage")]
        [Summary("Enables / Disabled the welcome and goodbye message")]
        [RequireOwner]
        public async Task ToggleWelcomeMessage([Remainder]SocketTextChannel channel = null)
        {
            var server = ServerLists.GetServer(Context.Guild);

            if (server.WelcomeMessageEnabled)
            {
                server.WelcomeMessageEnabled = false;
                await Context.Channel.SendMessageAsync("The welcome message was disabled");
            }
            else
            {
                if(channel == null && Bot._client.GetChannel(server.WelcomeChannel) != null)
                {
                    server.WelcomeMessageEnabled = true;
                    await Context.Channel.SendMessageAsync("The welcome message was enabled");
                }
                else if(channel != null)
                {
                    server.WelcomeMessageEnabled = true;
                    await Context.Channel.SendMessageAsync($"The welcome message was enabled and set the channel to **{channel.Name}**");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("You need to input a channel");
                }
            }

            ServerLists.SaveServerList();
        }
        
        [Command("setupwelcomemessage")]
        [Summary("Setups the welcome message and channel. Use [user] to mention the user. User [server] to insert the server name.")]
        [RequireOwner]
        public async Task SetupWelcomeMessage([Remainder]string message = "")
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.WelcomeMessage = message;

            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"The welcome message was set to '{message}'.");
        }

        [Command("setupgoodbye")]
        [Summary("Sets up the goodbye message")]
        [RequireOwner]
        public async Task SetupGoodbyeMessage([Remainder]string message = "")
        {
            var server = ServerLists.GetServer(Context.Guild);

            if (!string.IsNullOrWhiteSpace(message))
                server.WelcomeGoodbyeMessage = message;

            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync($"The goodbye message was set to '{message}'. For this to work the welcome message needs to be set up and enabled");
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
