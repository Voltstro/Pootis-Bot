using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Preconditions;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupWelcomeGoodbyeMessage : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Voltstro
		// Description      - Provides commands for setting up welcome channel
		// Contributors     - Voltstro, 

		[Command("setup welcomechannel")]
		[Alias("setup welcome channel")]
		[Summary("Sets where the custom welcome and goodbye messages will go")]
		[RequireGuildOwner]
		public async Task SetupWelcomeChannel([Remainder] SocketTextChannel channel = null)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (channel == null)
			{
				//The welcome/goodbye channel hasn't been set
				if (server.WelcomeChannelId == 0)
				{
					await Context.Channel.SendMessageAsync(
						"There is no welcome/goodbye message channel set! Run the command `setup welcomechannel [channel]` with a text channel name as the argument to set one up.");
					return;
				}

				//Disable the welcome and goodbye message and set their channel to 0
				server.WelcomeChannelId = 0;
				server.WelcomeMessageEnabled = false;
				server.GoodbyeMessageEnabled = false;
				ServerListsManager.SaveServerList();

				await Context.Channel.SendMessageAsync(
					"The custom welcome/goodbye message was disabled since no channel was provided when this command was ran.");
				return;
			}

			//Set the new channel
			server.WelcomeChannelId = channel.Id;
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"The custom welcome/goodbye messages will be put into the {channel.Mention} channel.");
		}

		#region Welcome/Goodbye Messages Toggles

		[Command("setup toggle welcomemessage")]
		[Summary("Enables/Disables the custom welcome message")]
		[RequireGuildOwner]
		public async Task ToggleWelcomeMessage()
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (server.WelcomeChannelId == 0)
			{
				await Context.Channel.SendMessageAsync(
					"You need to set a channel first for were the message will be put. Use the command `setup welcomechannel [?channel]` to set a channel.");
				return;
			}

			bool isWelcomeMessageEnabled = server.WelcomeMessageEnabled = !server.WelcomeMessageEnabled;
			ServerListsManager.SaveServerList();

			if (isWelcomeMessageEnabled)
				await Context.Channel.SendMessageAsync("The custom welcome message is enabled.");
			else
				await Context.Channel.SendMessageAsync("The custom welcome message is disabled.");
		}

		[Command("setup toggle goodbyemessage")]
		[Summary("Enables/Disables the custom goodbye message")]
		[RequireGuildOwner]
		public async Task ToggleGoodbyeMessage()
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (server.WelcomeChannelId == 0)
			{
				await Context.Channel.SendMessageAsync(
					"You need to set a channel first for were the message will be put. Use the command `setup welcomechannel [?channel]` to set a channel.");
				return;
			}

			bool isGoodbyeMessageEnabled = server.GoodbyeMessageEnabled = !server.GoodbyeMessageEnabled;
			ServerListsManager.SaveServerList();

			if (isGoodbyeMessageEnabled)
				await Context.Channel.SendMessageAsync("The custom goodbye message is enabled.");
			else
				await Context.Channel.SendMessageAsync("The custom goodbye message is disabled.");
		}

		#endregion

		#region Setting Welcome/Goodbye Message

		[Command("setup set welcomemessage")]
		[Summary("Sets the welcome message")]
		[RequireGuildOwner]
		public async Task SetWelcomeMessage([Remainder] string message = "")
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (string.IsNullOrWhiteSpace(message))
			{
				await Context.Channel.SendMessageAsync($"The current message is: `{server.WelcomeMessage}`");
				return;
			}

			server.WelcomeMessage = message;
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The message is now set to: `{message}`");
		}

		[Command("setup set goodbyemessage")]
		[Summary("Sets the goodbye message")]
		[RequireGuildOwner]
		public async Task SetGoodbyeMessage([Remainder] string message = "")
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (string.IsNullOrWhiteSpace(message))
			{
				await Context.Channel.SendMessageAsync($"The current message is: `{server.WelcomeGoodbyeMessage}`");
				return;
			}

			server.WelcomeGoodbyeMessage = message;
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The message is now set to: `{message}`");
		}

		#endregion
	}
}