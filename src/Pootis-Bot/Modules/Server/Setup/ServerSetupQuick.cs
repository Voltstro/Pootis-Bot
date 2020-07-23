using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services.Audio;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupQuick : ModuleBase<SocketCommandContext>
	{
		//Quick Rules Template
		private static string QuickRulesLocation => $"{Global.ResourcesDirectory}/quick-rules.txt";

		//Rules
		private const string RulesEmoji = "👌";

		private readonly OverwritePermissions everyoneChannelPermissions;

		private readonly GuildPermissions everyoneRoleGuildPermissions;

		//Permissions
		private readonly GuildPermissions memberRoleGuildPermissions;
		private readonly string quickRulesText;

		public ServerSetupQuick()
		{
			//Perms include: Create Instant Invite, View Channels, Send Messages, Embed Links, Attach Files, Read Message History, Add Reactions, Connect Speak
			memberRoleGuildPermissions = new GuildPermissions(3263553);

			//Perms: Add Reactions
			everyoneRoleGuildPermissions = new GuildPermissions(64);

			everyoneChannelPermissions = new OverwritePermissions(readMessageHistory: PermValue.Allow,
				viewChannel: PermValue.Allow, sendMessages: PermValue.Deny);

			if (!File.Exists(QuickRulesLocation))
				return;

			string rules = File.ReadAllText(QuickRulesLocation);

			if (!string.IsNullOrWhiteSpace(rules))
				quickRulesText = rules;
		}

		[Command("setup quick")]
		[Summary("Provides information on server quick setup")]
		[RequireGuildOwner(false)]
		public async Task SetupQuick()
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("Server Quick Setup");
			embed.WithDescription(
				"**What is server quick setup?**" +
				$"\nServer quick setup is a function in {Global.BotName} to quickly setup your server for you with a standard layout and using {Global.BotName}'s features." +
				"\n__This feature is designed for new servers!__" +
				"\n\n**So what does it do?**" +
				"\n1. Sets up a role called 'Member'." +
				$"\n2. It sets up a welcome channel (or uses one if it already exists) with {Global.BotName}'s custom welcome messages." +
				$"\n3. Sets up a 'rule' channel, and puts a template rule message there, or uses your provided ones. It then also sets up rule reaction with the '{RulesEmoji}' emoji." +
				"\n4. It then adds two categories called General and Gaming both with a text channel and an auto VC channel." +
				"\n\n**Getting started**" +
				"\nTo get started with a quick setup, run the command `setup quick start`. It will then do the steps listed above." +
				"\nIf you want to use custom rules, upload a `.txt` file with your rules in it, and as the note/message, put the command above." +
				"\n*To preview the template rules, do `setup quick rules`*" +
				"\n\n**Note**" +
				$"\nAll the features can also be manually done, you can read more about the features [here]({Global.websiteServerSetup}).");

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		[Command("setup quick rules")]
		[Summary("Displays the template rules")]
		[RequireGuildOwner(false)]
		public async Task SetupQuickRules()
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("Rules template preview");
			embed.WithDescription(string.IsNullOrEmpty(quickRulesText)
				? "Rules **MUST** be provided! This is due to how the bot is set up!"
				: $"Here is rules template that will be used if you don't provide rules:\n```{quickRulesText}```");

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		[Command("setup quick start", RunMode = RunMode.Async)]
		[Summary("Provides the ability to quickly setup your server with this bot")]
		[RequireGuildOwner(false)]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		[RequireBotPermission(GuildPermission.ManageChannels)]
		[RequireBotPermission(GuildPermission.AddReactions)]
		[RequireBotPermission(GuildPermission.ManageGuild)]
		public async Task SetupQuickStart()
		{
			await SetupServerQuick(Context.Guild, Context.Channel, Context.Message);
		}

		private async Task SetupServerQuick(SocketGuild guild, ISocketMessageChannel channel, SocketMessage message,
			bool addRulesMessage = true, bool setupWelcomeChannel = true,
			bool setupRuleReaction = true)
		{
			//Rules message
			string rules = quickRulesText;
			if (addRulesMessage)
			{
				//Check rules file, or if rules where provided
				if (message.Attachments.Count != 0)
				{
					Attachment attachment = message.Attachments.ElementAt(0);
					if (!attachment.Filename.EndsWith(".txt"))
					{
						await channel.SendMessageAsync("The provided rules file isn't in a `.txt` file format!");
						return;
					}

					rules = await Global.HttpClient.GetStringAsync(attachment.Url);
				}

				else if (string.IsNullOrWhiteSpace(rules) && message.Attachments.Count == 0)
				{
					await channel.SendMessageAsync("You MUST provide a rules file!");
					return;
				}
			}

			Logger.Debug("Running server quick setup on {@GuildName}({@GuildId})", guild.Name, guild.Id);

			ServerList server = ServerListsManager.GetServer(guild);

			//Setup the roles
			IRole memberRole = RoleUtils.GetGuildRole(guild, "Member");

			//There is already a role called "member"
			if (memberRole != null)
			{
				await memberRole.ModifyAsync(properties => properties.Permissions = memberRoleGuildPermissions);
			}
			else
			{
				//create a new role called member
				memberRole = await guild.CreateRoleAsync("Member", memberRoleGuildPermissions, Color.LightGrey, false,
					null);
			}

			//Modify @everyone role
			await guild.EveryoneRole.ModifyAsync(properties =>
				properties.Permissions = everyoneRoleGuildPermissions);

			//Setup the welcome channel
			if (setupWelcomeChannel)
			{
				ulong welcomeChannelId;

				//First, lets check if they already have a #welcome channel of sorts
				if (guild.SystemChannel == null)
				{
					//They don't, so set one up
					RestTextChannel welcomeChannel =
						await guild.CreateTextChannelAsync("welcome",
							properties =>
							{
								properties.Topic = "Were everyone gets a warm welcome!";
								properties.Position = 0;
							});

					await welcomeChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, everyoneChannelPermissions);

					welcomeChannelId = welcomeChannel.Id;
				}
				else
				{
					//They already do, so alter the pre-existing one to have right perms, and to not have Discord random messages put there
					await guild.SystemChannel.AddPermissionOverwriteAsync(guild.EveryoneRole,
						everyoneChannelPermissions);
					welcomeChannelId = guild.SystemChannel.Id;

					await guild.ModifyAsync(properties => properties.SystemChannel = null);
				}

				//Setup welcome message
				server.WelcomeChannelId = welcomeChannelId;
				server.WelcomeMessageEnabled = true;
				server.GoodbyeMessageEnabled = true;

				//Do a delay
				await Task.Delay(1000);
			}

			//Rule reaction
			if (addRulesMessage && setupRuleReaction)
			{
				//Setup rules channel
				RestTextChannel rulesChannel = await guild.CreateTextChannelAsync("rules",
					properties =>
					{
						properties.Position = 1;
						properties.Topic = "Rules of this Discord server";
					});

				await rulesChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, everyoneChannelPermissions);
				RestUserMessage rulesMessage = await rulesChannel.SendMessageAsync(rules);

				//Setup rules reaction
				await rulesMessage.AddReactionAsync(new Emoji(RulesEmoji));

				server.RuleReactionEmoji = RulesEmoji;
				server.RuleMessageId = rulesMessage.Id;
				server.RuleMessageChannelId = rulesChannel.Id;
				server.RuleRoleId = memberRole.Id;
				server.RuleEnabled = true;
			}

			//Setup the rest of the channels

			//General category
			await AddCategoryWithChannels(guild, memberRole, "General", 3);

			//Do a delay between categories
			await Task.Delay(500);

			//Gamming category
			await AddCategoryWithChannels(guild, memberRole, "Gaming", 4);

			//DONE!
			ServerListsManager.SaveServerList();
			await Context.Channel.SendMessageAsync("Quick Setup is done!");

			Logger.Debug("server quick setup on {@GuildName}({@GuildId}) is done!", guild.Name, guild.Id);
		}

		private static async Task AddCategoryWithChannels(SocketGuild guild, IRole memberRole, string categoryName,
			int position)
		{
			RestCategoryChannel category =
				await guild.CreateCategoryChannelAsync(categoryName, properties => properties.Position = position);
			await category.AddPermissionOverwriteAsync(guild.EveryoneRole, OverwritePermissions.InheritAll);
			await category.AddPermissionOverwriteAsync(memberRole, OverwritePermissions.InheritAll);

			//Text chat
			RestTextChannel chat = await guild.CreateTextChannelAsync(categoryName.ToLower(),
				properties => properties.CategoryId = category.Id);
			await chat.SyncPermissionsAsync();

			//Auto VC chat
			RestVoiceChannel autoVcChat = await AutoVCChannelCreator.CreateAutoVCChannel(guild, categoryName);
			await autoVcChat.ModifyAsync(properties => properties.CategoryId = category.Id);
			await autoVcChat.SyncPermissionsAsync();
		}
	}
}