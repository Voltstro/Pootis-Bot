using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pootis_Bot.Modules
{
    public class ServerSetup : ModuleBase<SocketCommandContext>
    {
        [Command("setup")]
        [RequireOwner]
        public async Task Setup()
        {
            var embed = new EmbedBuilder();

            embed.WithTitle("Setup Commands");
            embed.WithDescription($"\nSetup commands for {Config.bot.botName}.\n**ALL OF THESE COMMAND CAN ONLY BE EXCUTED BY THE OWNER OF THE SERVER!**\n\n" +
                $"'setupwelcomeid [Welcome ChannelID]' -- Use this to setup the welcome channel ID\n" +
                $"'togglewelcome' -- Toggels between enabling the welcome message or not\n" +
                $"'togglerules' -- In the welcome chat it will say 'check out #rules'. Do you want that?\n" +
                $"'setupadmin [Admin Role Name]' -- The admin role name" +
                $"\n'setupstaff [Staff Role Name]' -- The staff role name" +
                $"");
            embed.WithColor(new Color(255, 81, 168));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }           

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

        [Command("togglerules")]
        [RequireOwner]
        public async Task ToggleRules()
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.enableWelcome = server.isRules = !server.isRules;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync("Rules was set to " + server.isRules);
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

        [Command("setupstaff")]
        [RequireOwner]
        public async Task SetupStaff(string staffRoleName)
        {
            var server = ServerLists.GetServer(Context.Guild);
            server.staffRoleName = staffRoleName;
            ServerLists.SaveServerList();

            await Context.Channel.SendMessageAsync("Staff role was set to " + staffRoleName);
        }
    }
}
