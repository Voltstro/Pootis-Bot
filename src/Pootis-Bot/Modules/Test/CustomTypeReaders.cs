#if DEBUG

using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;

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
	}
}

#endif