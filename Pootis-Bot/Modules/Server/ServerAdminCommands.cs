using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Pootis_Bot.Modules.Server
{
    public class ServerAdminCommands : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        [Summary("Kicks a user")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickUser(IGuildUser user, [Remainder]string reason = "")
        {
            await user.KickAsync(reason);
            await Context.Channel.SendMessageAsync($"The user {user.Username} was kicked");
        }

        [Command("ban")]
        [Summary("Bans a user")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUser(IGuildUser user, int days = 0, [Remainder]string reason = "")
        {
            await user.BanAsync(days, reason);
            await Context.Channel.SendMessageAsync($"The user {user.Username} was banned");
        }
    }
}