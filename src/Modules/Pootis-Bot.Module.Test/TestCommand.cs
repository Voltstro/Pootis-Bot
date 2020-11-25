using System.Threading.Tasks;
using Discord.Commands;

namespace Pootis_Bot.Module.Test
{
	public class TestCommand : ModuleBase<SocketCommandContext>
	{
		[Command("test")]
		public async Task Test()
		{
			await Context.Channel.SendMessageAsync("Hello");
		}
	}
}