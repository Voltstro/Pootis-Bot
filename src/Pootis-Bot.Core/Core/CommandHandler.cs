using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Config;

namespace Pootis_Bot.Core
{
	/// <summary>
	///     Handles commands for Discord
	/// </summary>
	internal sealed class CommandHandler
	{
		private readonly DiscordSocketClient client;
		private readonly CommandService commandService;
		private readonly BotConfig config;

		/// <summary>
		///     Creates a new <see cref="CommandHandler" /> instance
		/// </summary>
		/// <param name="client"></param>
		internal CommandHandler(DiscordSocketClient client)
		{
			client.MessageReceived += HandleMessage;
			this.client = client;
			commandService = new CommandService();
			config = Config<BotConfig>.Instance;
		}

		/// <summary>
		///     Install modules in an assembly
		/// </summary>
		/// <param name="assembly"></param>
		internal void InstallAssemblyModules(Assembly assembly)
		{
			commandService.AddModulesAsync(assembly, null);
		}

		private async Task HandleMessage(SocketMessage msg)
		{
			//Check the message first
			if (!CheckMessage(msg, out SocketUserMessage userMessage, out SocketCommandContext context)) return;

			//Does the message start with the prefix or mention of the bot
			int argPos = 0;
			if (!userMessage.HasStringPrefix(config.BotPrefix, ref argPos) &&
			    !userMessage.HasMentionPrefix(client.CurrentUser, ref argPos)) return;

			//Execute the command
			IResult result = await commandService.ExecuteAsync(context, argPos, null);

			//Handle it result
			if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
				await context.Channel.SendMessageAsync("You do not meet the conditions to use that command!");
		}

		private bool CheckMessage(SocketMessage message, out SocketUserMessage msg, out SocketCommandContext context)
		{
			msg = null;
			context = null;

			if (!(message is SocketUserMessage userMessage)) return false;
			msg = userMessage;

			context = new SocketCommandContext(client, msg);

			if (message.Author.IsBot || message.Author.IsWebhook)
				return false;

			return true;
		}
	}
}