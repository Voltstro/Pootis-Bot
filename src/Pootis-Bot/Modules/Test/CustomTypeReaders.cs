#if DEBUG

using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Pootis_Bot.Modules.Test
{
	public class CustomTypeReaders : ModuleBase<SocketCommandContext>
	{
		[Command("testguildusers")]
		public async Task TestGuildUsers([Remainder] SocketGuildUser[] users)
		{
			StringBuilder sb = new StringBuilder();
			foreach (SocketGuildUser user in users) sb.Append(user.Username + ", ");

			await Context.Channel.SendMessageAsync(sb.ToString());
		}

		[Command("emoji")]
		public async Task TestEmoji([Remainder] Emoji emoji)
		{
			await Context.Channel.SendMessageAsync($"Emoji is a emoji! {emoji.Name}");
		}
	}
}

#endif