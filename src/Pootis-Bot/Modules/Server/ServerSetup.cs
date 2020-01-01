using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Structs.Server;

namespace Pootis_Bot.Modules.Server
{
	public class ServerSetup : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - Helps the server owner set up the bot for use
		// Contributors     - Creepysin, 

		[Command("togglementionuser")]
		[Alias("toggle mention user")]
		[Summary("Enables / Disables the mention user anti-spam feature")]
		[RequireGuildOwner]
		public async Task ToggleMentionUserSpam()
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			server.AntiSpamSettings.MentionUserEnabled = !server.AntiSpamSettings.MentionUserEnabled;

			ServerListsManager.SaveServerList();
			await Context.Channel.SendMessageAsync(
				$"Mention user anti-spam was set to {server.AntiSpamSettings.MentionUserEnabled}.");
		}

		[Command("setmentionuserthreshold")]
		[Alias("set mention user threshold")]
		[Summary("Set how much of a percentage of a servers users need to be mention before it is considered spam")]
		[RequireGuildOwner]
		public async Task SetMentionUserThreshold(int threshold)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			server.AntiSpamSettings.MentionUsersPercentage = threshold;

			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"The threshold for amount of users in a message was set to {threshold}.");
		}

		[Command("togglewelcomemessage")]
		[Alias("toggle welcome message")]
		[Summary("Enables / Disabled the welcome and goodbye message")]
		[RequireGuildOwner]
		public async Task ToggleWelcomeMessage([Remainder] SocketTextChannel channel = null)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			//The welcome message is enabled, so disable it
			if (server.WelcomeMessageEnabled)
			{
				server.WelcomeMessageEnabled = false;

				ServerListsManager.SaveServerList();

				await Context.Channel.SendMessageAsync("The welcome message was disabled.");
			}
			else if (!server.WelcomeMessageEnabled) //The welcome message is not enabled
			{
				if (channel == null)
				{
					if (Context.Client.GetChannel(server.WelcomeChannelId) != null)
					{
						server.WelcomeMessageEnabled = true;
						await Context.Channel.SendMessageAsync(
							$"The welcome channel was enabled and set to {((SocketTextChannel) Context.Client.GetChannel(server.WelcomeChannelId)).Mention}");

						ServerListsManager.SaveServerList();
					}
					else
					{
						await Context.Channel.SendMessageAsync(
							$"You need to input a channel name! E.G: `{Global.BotPrefix}togglewelcomemessage welcome`");
					}
				}
				else
				{
					//Set the welcome channel to the inputed one
					server.WelcomeMessageEnabled = true;
					server.WelcomeChannelId = channel.Id;

					ServerListsManager.SaveServerList();

					await Context.Channel.SendMessageAsync(
						$"The welcome channel was enabled and set to {channel.Mention}");
				}
			}
		}

		[Command("togglegoodbyemessage")]
		[Alias("toggle goodbye message")]
		[Summary("Enables/Disables the goodbye message")]
		[RequireGuildOwner]
		public async Task ToggleGoodbyeMessage()
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (server.GoodbyeMessageEnabled)
				server.GoodbyeMessageEnabled = false;
				
			else
			{
				if (Context.Client.GetChannel(server.WelcomeChannelId) == null)
				{
					await Context.Channel.SendMessageAsync("The welcome channel isn't set yet or is invalid!");
					return;
				}

				server.GoodbyeMessageEnabled = true;
			}
				
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"Goodbye message was set to **{server.GoodbyeMessageEnabled.ToString().ToLower()}**.");
		}

		[Command("setupwelcomemessage")]
		[Alias("setup welcome message")]
		[Summary(
			"Setups the welcome message and channel. Use [user] to mention the user. User [server] to insert the server name.")]
		[RequireGuildOwner]
		public async Task SetupWelcomeMessage([Remainder] string message = "")
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			server.WelcomeMessage = message;

			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The welcome message was set to '{message}'.");
		}

		[Command("setupgoodbyemessage")]
		[Alias("setup googbye message")]
		[Summary("Sets up the goodbye message")]
		[RequireGuildOwner]
		public async Task SetupGoodbyeMessage([Remainder] string message = "")
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (!string.IsNullOrWhiteSpace(message))
				server.WelcomeGoodbyeMessage = message;

			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"The goodbye message was set to '{message}'. For this to work the welcome message needs to be set up and enabled");
		}

		[Command("setuprulesmessage")]
		[Summary("Sets the message that needs to be reacted to. You need to run the command in the same channel were the message is located!")]
		[Alias("setup rules message")]
		[RequireGuildOwner]
		public async Task SetupRuleMessage(ulong id = 0)
		{
			if (id != 0)
			{
				if (await Context.Channel.GetMessageAsync(id) != null)
				{
					ServerList server = ServerListsManager.GetServer(Context.Guild);
					server.RuleMessageId = id;
					server.RuleMessageChannelId = Context.Channel.Id;

					ServerListsManager.SaveServerList();

					await Context.Channel.SendMessageAsync(
						$"The rule message was set to the message with the ID of **{id}**.");
				}
				else
				{
					await Context.Channel.SendMessageAsync(
						"Cannot find a message with that ID! Make sure you are in the same channel were the message is located.");
				}
			}
			else
			{
				await Context.Channel.SendMessageAsync("The rules message was disabled.");
				ServerList server = ServerListsManager.GetServer(Context.Guild);
				server.RuleMessageId = 0;

				ServerListsManager.SaveServerList();
			}
		}

		[Command("setupruleemoji")]
		[Summary(
			"Sets the rule reaction emoji. MUST BE UNICODE. Refer to here for more details. https://unicode.org/emoji/charts/full-emoji-list.html")]
		[Alias("setup rule emoji")]
		[RequireGuildOwner]
		public async Task SetupRuleEmoji(string emoji)
		{
			if (!Global.ContainsUnicodeCharacter(emoji))
			{
				await Context.Channel.SendMessageAsync(
					"This emoji is not unicode. Copy the emoji you want to be reacted with from here: https://unicode.org/emoji/charts/full-emoji-list.html");
			}
			else
			{
				ServerList server = ServerListsManager.GetServer(Context.Guild);
				server.RuleReactionEmoji = emoji;
				ServerListsManager.SaveServerList();

				await Context.Channel.SendMessageAsync("The emoji was set to " + emoji);
			}
		}

		[Command("togglerulereaction")]
		[Summary("Enables or disables the rule reaction feature is on or off")]
		[Alias("toggle rule reaction")]
		[RequireGuildOwner]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task ToggleRuleReaction()
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			//The rule reaction is enabled, disable it
			if (server.RuleEnabled)
			{
				server.RuleEnabled = false;
				ServerListsManager.SaveServerList();

				await Context.Channel.SendMessageAsync("The rule reaction feature was disabled.");
			}
			else //Enable the rule reaction feature, but first check to make sure everything else is setup first and is correct
			{
				//First, lets make sure the rule role is still valid and exists
				if (RoleUtils.GetGuildRole(Context.Guild, server.RuleRoleId) != null)
				{
					if (server.RuleMessageId != 0)
					{
						//Now lets double check if the rule emoji is valid emoji
						if (Global.ContainsUnicodeCharacter(server.RuleReactionEmoji))
						{
							//If we reach here, then we are all good to go!
							server.RuleEnabled = true;

							ServerListsManager.SaveServerList();

							await Context.Channel.SendMessageAsync("The rule reaction feature is enabled.");
						}
						else
						{
							//The emoji was incorrect or wasn't set
							await Context.Channel.SendMessageAsync(
								"The emoji isn't set or is incorrect, use the command `setupruleemoji` to set the emoji.");
						}
					}
					else
					{
						//The rule message either doesn't exist anymore, or was never set in the first place
						await Context.Channel.SendMessageAsync(
							"The rule message id isn't set, use the command `setuprulesmessage` to set the rule message id.");
					}
				}
				else
				{
					//The rule role doesn't exist or wasn't set
					await Context.Channel.SendMessageAsync(
						"The reaction role was either no set, or no longer exist, use the command `setuprulerole` to set the role.");
				}
			}
		}

		[Command("setuprulerole")]
		[Summary("Sets the role to give to the user after they have reacted")]
		[RequireGuildOwner]
		public async Task SetupRuleRole([Remainder] string roleName)
		{
			//Get the role and check to see if it exists
			SocketRole role = RoleUtils.GetGuildRole(Context.Guild, roleName);

			if (role != null)
			{
				ServerListsManager.GetServer(Context.Guild).RuleRoleId = role.Id;
				ServerListsManager.SaveServerList();

				await Context.Channel.SendMessageAsync($"The rule role was set to **{role.Name}**.");
			}
			else
			{
				await Context.Channel.SendMessageAsync($"{roleName} doesn't exist in the guild!");
			}
		}

		[Command("addrolepoints")]
		[Alias("addpointsrole", "add points role", "add role points")]
		[Summary("Adds a role to give after a user gets a certain amount of points")]
		[RequireGuildOwner]
		public async Task AddRolePoints(uint pointsAmount, string roleName)
		{
			SocketRole role = RoleUtils.GetGuildRole(Context.Guild, roleName);
			if (role == null)
			{
				await Context.Channel.SendMessageAsync("That role doesn't exists!");
				return;
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (pointsAmount >= server.PointGiveAmount)
			{
				await Context.Channel.SendMessageAsync(
					$"Sorry, but you have to set the points require higher then {server.PointGiveAmount}");

				return;
			}

			ServerRolePoints serverRolePoints = new ServerRolePoints
			{
				RoleId = role.Id,
				PointsRequired = pointsAmount
			};

			server.ServerRolePoints.Add(serverRolePoints);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync(
				$"Ok, when a user gets {pointsAmount} points they shell receive the **{role.Name}** role.\nPlease note any user who already have {pointsAmount} points won't get the role.");
		}

		[Command("removerolepoints")]
		[Alias("removepointsrole", "remove points role", "remove role points")]
		[Summary("Removes a role to give to a user after they get a certain amount of points")]
		[RequireGuildOwner]
		public async Task RemoveRolePoints(uint pointsAmount)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			ServerRolePoints serverRolePoints = server.GetServerRolePoints(pointsAmount);
			if (serverRolePoints.PointsRequired == 0)
			{
				await Context.Channel.SendMessageAsync(
					$"There are no role points that have {pointsAmount} points as there requirement!");
				return;
			}

			server.ServerRolePoints.Remove(serverRolePoints);
			ServerListsManager.SaveServerList();

			//TODO: Maybe a better reply?
			await Context.Channel.SendMessageAsync(
				$"The {serverRolePoints.PointsRequired} points required one was removed.");
		}

		[Command("rolepoints")]
		[Alias("pointsrole", "role points", "points role")]
		[Summary("Gets all the role points")]
		[RequireGuildOwner]
		public async Task RolePoints()
		{
			StringBuilder sb = new StringBuilder();
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			sb.Append("__**Server Role Points**__\n");

			foreach (ServerRolePoints rolePoint in server.ServerRolePoints)
				sb.Append(
					$"[{RoleUtils.GetGuildRole(Context.Guild, rolePoint.RoleId)}] **{rolePoint.PointsRequired}**\n");

			await Context.Channel.SendMessageAsync(sb.ToString());
		}

		[Command("rolegiveadd")]
		[Alias("role give add", "add role give")]
		[Summary("Assigns you a specified role if the user meets a requirement")]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		public async Task RoleGiveAdd(string roleGiveName, string roleToGive, [Remainder] string roleRequired = "")
		{
			SocketRole roleToAssign = RoleUtils.GetGuildRole(Context.Guild, roleToGive);

			//Check to make sure the role exists first
			if (roleToAssign == null)
			{
				await Context.Channel.SendMessageAsync($"No role under the name '{roleToGive}' exists!");
				return;
			}

			SocketRole socketRoleRequired = null;

			//If a required role was specified, check to make sure it exists
			if (!string.IsNullOrWhiteSpace(roleRequired))
			{
				socketRoleRequired = RoleUtils.GetGuildRole(Context.Guild, roleRequired);
				if (socketRoleRequired == null)
				{
					await Context.Channel.SendMessageAsync($"Role {roleRequired} doesn't exist!");
					return;
				}
			}

			ServerList server = ServerListsManager.GetServer(Context.Guild);

			//Check to make sure a role give doesn't already exist first
			if (server.GetRoleGive(roleGiveName) != null)
			{
				await Context.Channel.SendMessageAsync($"A role give with the name '{roleGiveName}' already exist!");
				return;
			}

			RoleGive roleGive = new RoleGive
			{
				Name = roleGiveName,
				RoleToGiveId = roleToAssign.Id,
				RoleRequiredId = 0
			};

			if (socketRoleRequired != null)
				roleGive.RoleRequiredId = socketRoleRequired.Id;

			server.RoleGives.Add(roleGive);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"The role give was created with the name of **{roleGiveName}**.");
		}

		[Command("rolegiveremove")]
		[Alias("role give remove", "remove role give")]
		[Summary("Removes a role give")]
		public async Task RoleGiveRemove(string roleGiveName)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			RoleGive roleGive = server.GetRoleGive(roleGiveName);
			if (roleGive == null)
			{
				await Context.Channel.SendMessageAsync($"There is no role give with the name '{roleGiveName}''.");
				return;
			}

			server.RoleGives.Remove(roleGive);
			ServerListsManager.SaveServerList();

			await Context.Channel.SendMessageAsync($"Removed role give '{roleGiveName}'.'");
		}
	}
}