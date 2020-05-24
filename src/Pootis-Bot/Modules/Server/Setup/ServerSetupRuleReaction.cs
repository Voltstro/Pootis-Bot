using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupRuleReaction : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Provides commands for setting up the rule reaction feature
		// Contributors     - Creepysin, 

		#region Inital Rule Reaction Setup Commands

		[Command("setup set rulemessage")]
		[Summary(
			"Sets what message that users need to react to. Run this command in the same channel as were the message is.")]
		[RequireBotPermission(GuildPermission.ReadMessageHistory)]
		[RequireBotPermission(GuildPermission.ViewChannel)]
		[RequireGuildOwner]
		public async Task SetRuleMessage(ulong id = 0, bool silenceMessage = false)
		{
			if (id != 0)
			{
				if (await Context.Channel.GetMessageAsync(id) != null)
				{
					ServerList server = ServerListsManager.GetServer(Context.Guild);
					server.RuleMessageId = id;
					server.RuleMessageChannelId = Context.Channel.Id;

					ServerListsManager.SaveServerList();

					if (!silenceMessage)
						await Context.Channel.SendMessageAsync(
							$"The rule message was set to the message with the ID of **{id}**.");
					else
						await Context.Message.DeleteAsync();
				}
				else
				{
					await Context.Channel.SendMessageAsync(
						"Cannot find a message with that ID! Make sure you are in the same channel were the message is located.");
				}
			}
			else
			{
				ServerList server = ServerListsManager.GetServer(Context.Guild);
				server.RuleMessageId = 0;
				server.RuleEnabled = false;

				ServerListsManager.SaveServerList();

				await Context.Channel.SendMessageAsync("The rules message was removed.");
			}
		}

		[Command("setup set rulerole")]
		[Summary("Sets what role to give once a user successfully reacts to the rule message")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		[RequireGuildOwner]
		public async Task SetRuleRole([Remainder] string roleName = "")
		{
			if (string.IsNullOrWhiteSpace(roleName)) //Make sure the user actually provided a role
			{
				await Context.Channel.SendMessageAsync("You need to provide a role name!");
				return;
			}

			SocketRole role = RoleUtils.GetGuildRole(Context.Guild, roleName);
			if (role == null) //Make sure the role exists
			{
				await Context.Channel.SendMessageAsync($"The role **{roleName}** doesn't exist!");
				return;
			}

			//Modify the server settings to update for the new role
			ServerListsManager.GetServer(Context.Guild).RuleRoleId = role.Id;
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"The role that users shell receive after successfully reacting to the rules will be the **{role.Name}** role.");
		}

		[Command("setup set ruleemoji")]
		[Summary("Sets the emoji that users have to use to gain access")]
		[RequireGuildOwner]
		public async Task SetRuleEmoji([Remainder] Emoji emoji)
		{
			ServerListsManager.GetServer(Context.Guild).RuleReactionEmoji = emoji.Name;
			ServerListsManager.SaveServerList();

			Logger.Log(emoji.Name);
			await Context.Channel.SendMessageAsync($"The emoji was set to '{emoji.Name}'.");
		}

		#endregion

		[Command("setup toggle rulereaction")]
		[Summary("Enables/Disables the rule reaction feature. All the other commands MUST be ran before this one.")]
		[RequireBotPermission(GuildPermission.AddReactions)]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		[RequireBotPermission(GuildPermission.ReadMessageHistory)]
		[RequireBotPermission(GuildPermission.ViewChannel)]
		[RequireGuildOwner]
		public async Task ToggleRuleReaction()
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			//If the rule reaction feature is already enabled, disable it
			if (server.RuleEnabled)
			{
				server.RuleEnabled = false;
				ServerListsManager.SaveServerList();

				await Context.Channel.SendMessageAsync("The rule reaction feature is now disabled.");
				return;
			}

			//Make sure the rule message channel still exists
			SocketTextChannel ruleChannel = Context.Guild.GetTextChannel(server.RuleMessageChannelId);
			if (ruleChannel == null)
			{
				server.RuleMessageChannelId = 0; //Reset it back to 0 so we don't have to write it
				ServerListsManager.SaveServerList();

				return;
			}

			//Make sure the message still exists
			IMessage rulesMessage = await ruleChannel.GetMessageAsync(server.RuleMessageId);
			if (rulesMessage == null)
			{
				await Context.Channel.SendMessageAsync($"The rules message that is meant to belong in the {ruleChannel.Mention} channel doesn't exist anymore!");
				return;
			}

			//Check the emoji
			if (!server.RuleReactionEmoji.ContainsOnlyOneEmoji())
			{
				await Context.Channel.SendMessageAsync("The emoji that is meant to be used is invalid!");
				return;
			}

			//Ok, everything is all good to go, now just to enable it

			//First add our reaction
			IUserMessage rulesMessageUser = (IUserMessage) rulesMessage;
			await rulesMessageUser.AddReactionAsync(new Emoji(server.RuleReactionEmoji));

			//And actually enable the feature
			server.RuleEnabled = true;
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync("The rule reaction feature is now enabled!");
		}
	}
}