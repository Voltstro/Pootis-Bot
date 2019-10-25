using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Modules.Server
{
	public class ServerSetup : ModuleBase<SocketCommandContext>
	{
		// Module Information
		// Original Author   - Creepysin
		// Description      - Helps the server owner set up the bot for use
		// Contributors     - Creepysin, 

		[Command("setup")]
		[Summary("Displays setup info")]
		[RequireGuildOwner]
		public async Task Setup()
		{
			IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			EmbedBuilder embed = new EmbedBuilder();

			await Context.Channel.SendMessageAsync("Setup status was sent to your dms.");

			embed.WithTitle("Setup Status");
			embed.WithColor(new Color(255, 81, 168));
			embed.WithDescription(
				$"<:Menu:537572055760109568> Here is your setup status for **{Context.Guild.Name}**.\nSee [here]({Global.websiteServerSetup}) for more help.\n\n");
			embed.WithThumbnailUrl(Context.Guild.IconUrl);
			embed.WithCurrentTimestamp();

			string welcometitle = "<:Cross:537572008574189578> Welcome Channel Disabled"; // Welcome Message and channel
			string welocmedes = "Welcome channel is disabled\n";
			if (server.WelcomeMessageEnabled)
			{
				welcometitle = "<:Check:537572054266806292> Welcome Channel Enabled";
				welocmedes =
					$"Welcome channel is enabled and is set to the channel **{((SocketTextChannel) Context.Client.GetChannel(server.WelcomeChannelId)).Name}**\n";
			}

			embed.AddField(welcometitle, welocmedes);

			string rulereactiontitle = "<:Cross:537572008574189578> Rule Reaction Disabled"; // Rule Reaction feature
			string rulereactiondes = "Rule reaction is disabled.\n";
			if (server.RuleEnabled)
			{
				rulereactiontitle = "<:Check:537572054266806292> Rule Reaction Enabled";
				rulereactiondes =
					$"The rule reaction feature is enabled and is set to the message id '{server.RuleMessageId}' with the emoji '{server.RuleReactionEmoji}'";
			}

			embed.AddField(rulereactiontitle, rulereactiondes);

			string warningTitle = "Warnings"; // Warnings
			string warningDes = "";
			if (server.GetCommandInfo("warn") == null)
				warningDes += "<:Cross:537572008574189578> The command `warn` doesn't have a permission added to it!\n";
			if (server.GetCommandInfo("makewarnable") == null)
				warningDes +=
					"<:Cross:537572008574189578> The command `makewarnable` doesn't have a permission added to it!\n";
			if (server.GetCommandInfo("makenotwarnable") == null)
				warningDes +=
					"<:Cross:537572008574189578> The command `makenotwarnable` doesn't have a permission added to it!\n";
			if (server.GetCommandInfo("ban") == null)
				warningDes += "<:Cross:537572008574189578> The command `ban` doesn't have a permission added to it!\n";
			if (server.GetCommandInfo("kick") == null)
				warningDes += "<:Cross:537572008574189578> The command `kick` doesn't have a permission added to it!\n";
			if(server.GetCommandInfo("mute") == null)
				warningDes += "<:Cross:537572008574189578> The command `mute` doesn't have a permission added to it!\n";
			else
				warningDes = "You have no warnings! :smile:";
			embed.AddField(warningTitle, warningDes);

			embed.WithFooter($"For support see {Global.websiteHome}", Context.Client.CurrentUser.GetAvatarUrl());

			await dm.SendMessageAsync("", false, embed.Build());
		}

		[Command("setup spam")]
		[Summary("Shows setup info regarding the server's anti-spam settings")]
		[RequireGuildOwner]
		public async Task SetupSpam()
		{
			IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
			ServerList server = ServerListsManager.GetServer(Context.Guild);
			EmbedBuilder embed = new EmbedBuilder();

			await Context.Channel.SendMessageAsync("Setup anti-spam status was sent to your dms.");

			embed.WithTitle("Anti-Spam Setup Status");
			embed.WithColor(new Color(255, 81, 168));
			embed.WithDescription(
				$"<:Menu:537572055760109568> Here is your anti-spam setup status for **{Context.Guild.Name}**.\nSee [here]({Global.websiteServerSetup}) for more help.\n\n");
			embed.WithThumbnailUrl(Context.Guild.IconUrl);
			embed.WithCurrentTimestamp();

			string mentionUserTitle = "<:Cross:537572008574189578> Mention user spam is disabled!";
			string mentionUserDes =
				$"If a user with more then {server.AntiSpamSettings.MentionUsersPercentage}% of the server's users are mentioned, they will be warned.";
			if (server.AntiSpamSettings.MentionUserEnabled)
				mentionUserTitle = "<:Check:537572054266806292> Mention user spam is enabled!";

			embed.AddField(mentionUserTitle, mentionUserDes);
			embed.AddField("Role to Role mention",
				$"{server.AntiSpamSettings.RoleToRoleMentionWarnings} mentions of the same user will result in one warning");

			await dm.SendMessageAsync("", false, embed.Build());
		}

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

			await Context.Channel.SendMessageAsync($"The threshold was set to {threshold}.");
		}

		[Command("togglewelcomemessage")]
		[Alias("toggle welcome message")]
		[Summary("Enables / Disabled the welcome and goodbye message")]
		[RequireGuildOwner]
		public async Task ToggleWelcomeMessage([Remainder] SocketTextChannel channel = null)
		{
			ServerList server = ServerListsManager.GetServer(Context.Guild);

			if (server.WelcomeMessageEnabled && (channel == null))
			{
				//Disable the welcome message
				server.WelcomeMessageEnabled = false;

				ServerListsManager.SaveServerList();

				await Context.Channel.SendMessageAsync("The welcome message was disabled.");
			}
			else if (!server.WelcomeMessageEnabled)
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
					//Set the welcome channel to the imputed one
					server.WelcomeMessageEnabled = true;
					server.WelcomeChannelId = channel.Id;

					ServerListsManager.SaveServerList();

					await Context.Channel.SendMessageAsync(
						$"The welcome channel was enabled and set to {channel.Mention}");
				}
			}
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
		[Summary("Sets the rules message by the message id. Needs to be in the same channel with the message!")]
		[Alias("setup rules message")]
		[RequireGuildOwner]
		public async Task SetupRuleMessage(ulong id = 0)
		{
			if (id != 0)
			{
				if (Context.Channel.GetMessageAsync(id) != null)
				{
					ServerList server = ServerListsManager.GetServer(Context.Guild);
					server.RuleMessageId = id;

					ServerListsManager.SaveServerList();

					await Context.Channel.SendMessageAsync(
						$"The rule message was set to the message with the id of **{id}**.");
				}
				else
				{
					await Context.Channel.SendMessageAsync("That message doesn't exist");
				}
			}
			else
			{
				await Context.Channel.SendMessageAsync("The rules message was disabled");
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
				if (Global.GetGuildRole(Context.Guild, server.RuleRoleId) != null) 
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
			SocketRole role = Global.GetGuildRole(Context.Guild, roleName);

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
			SocketRole role = Global.GetGuildRole(Context.Guild, roleName);
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
			{
				sb.Append($"[{Global.GetGuildRole(Context.Guild, rolePoint.RoleId)}] **{rolePoint.PointsRequired}**\n");
			}

			await Context.Channel.SendMessageAsync(sb.ToString());
		}
	}
}