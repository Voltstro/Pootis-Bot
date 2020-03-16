using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services.Audio;

namespace Pootis_Bot.Modules.Server.Setup
{
	public class ServerSetupQuick : ModuleBase<SocketCommandContext>
	{
		//Permissions
		private readonly GuildPermissions _memberRoleGuildPermissions;
		private readonly GuildPermissions _everyoneRoleGuildPermissions;

		private readonly OverwritePermissions _everyoneChannelPermissions;

		//Quick Rules Template
		private const string QuickRulesLocation = "Resources/quick-rules.txt";
		private readonly string _quickRulesText;

		//Rules
		private const string RulesEmoji = "👌";
		
		public ServerSetupQuick()
		{
			//Perms include: Create Instant Invite, View Channels, Send Messages, Embed Links, Attach Files, Read Message History, Add Reactions, Connect Speak
			_memberRoleGuildPermissions = new GuildPermissions(3263553);
			_everyoneRoleGuildPermissions = new GuildPermissions(64);

			_everyoneChannelPermissions = new OverwritePermissions(readMessageHistory:PermValue.Allow, viewChannel:PermValue.Allow, sendMessages:PermValue.Deny);

			if(!File.Exists(QuickRulesLocation))
				return;

			string rules = File.ReadAllText(QuickRulesLocation);

			if (!string.IsNullOrWhiteSpace(rules))
				_quickRulesText = rules;
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
				"\nIt is designed for new servers.");

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		[Command("setup quick start")]
		[Summary("Provides the ability to quickly setup")]
		[RequireGuildOwner(false)]
		[RequireBotPermission(GuildPermission.ManageRoles)]
		[RequireBotPermission(GuildPermission.ManageChannels)]
		[RequireBotPermission(GuildPermission.AddReactions)]
		[RequireBotPermission(GuildPermission.ManageGuild)]
		public async Task SetupQuickStart()
		{
			//Check rules file, or if rules where provided
			string rules = _quickRulesText;
			if (Context.Message.Attachments.Count != 0)
			{
				Attachment attachment = Context.Message.Attachments.ElementAt(0);
				if (!attachment.Filename.EndsWith(".txt"))
				{
					await Context.Channel.SendMessageAsync("The provided rules file isn't in a `.txt` file format!");
					return;
				}

				rules = await Global.HttpClient.GetStringAsync(attachment.Url);
			}

			else if (string.IsNullOrWhiteSpace(rules) && Context.Message.Attachments.Count == 0)
			{
				await Context.Channel.SendMessageAsync("You MUST provide a rules file!");
				return;
			}

			SocketGuild guild = Context.Guild;
			ServerList server = ServerListsManager.GetServer(guild);

			//Setup the roles
			IRole memberRole = RoleUtils.GetGuildRole(guild, "Member");

			//There is already a role called "member"
			if (memberRole != null)
			{
				await memberRole.ModifyAsync(properties => properties.Permissions = _memberRoleGuildPermissions);
			}
			else
			{
				//create a new role called member
				memberRole = await guild.CreateRoleAsync("Member", _memberRoleGuildPermissions);
				await guild.EveryoneRole.ModifyAsync(properties => properties.Permissions = _everyoneRoleGuildPermissions);
			}

			//Setup the channels
			ulong welcomeChannelId;

			//First, lets check if they already have a #welcome channel of sorts
			if (guild.SystemChannel == null)
			{
				//They don't, so set one up
				RestTextChannel welcomeChannel =
					await guild.CreateTextChannelAsync("welcome",
						properties => { properties.Topic = "Were everyone gets a warm welcome!";
							properties.Position = 0;
						});

				await welcomeChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, _everyoneChannelPermissions);

				welcomeChannelId = welcomeChannel.Id;
			}
			else
			{
				await guild.SystemChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, _everyoneChannelPermissions);
				welcomeChannelId = guild.SystemChannel.Id;

				await guild.ModifyAsync(properties => properties.SystemChannel = null);
			}

			//Setup welcome message
			server.WelcomeChannelId = welcomeChannelId;
			server.WelcomeMessageEnabled = true;

			//Do a delay
			await Task.Delay(1000);

			//Setup rules channel
			RestTextChannel rulesChannel = await guild.CreateTextChannelAsync("rules",
				properties => { properties.Position = 1;
					properties.Topic = "Rules of this Discord server";
				});

			await rulesChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, _everyoneChannelPermissions);
			RestUserMessage rulesMessage = await rulesChannel.SendMessageAsync(rules);

			//Setup rules reaction
			await rulesMessage.AddReactionAsync(new Emoji(RulesEmoji));

			server.RuleReactionEmoji = RulesEmoji;
			server.RuleMessageId = rulesMessage.Id;
			server.RuleMessageChannelId = rulesMessage.Id;
			server.RuleRoleId = memberRole.Id;
			server.RuleEnabled = true;

			//Setup the rest of the channels

			//General category
			RestCategoryChannel generalCategory = await guild.CreateCategoryChannelAsync("General", properties => properties.Position = 3);
			await generalCategory.AddPermissionOverwriteAsync(guild.EveryoneRole, OverwritePermissions.InheritAll);
			await generalCategory.AddPermissionOverwriteAsync(memberRole, OverwritePermissions.InheritAll);

			await guild.CreateTextChannelAsync("general", properties => properties.CategoryId = generalCategory.Id);

			RestVoiceChannel generalVcChannel = await AutoVCChannelCreator.CreateAutoVCChannel(guild, "General");
			await generalVcChannel.ModifyAsync(properties => properties.CategoryId = generalCategory.Id);

			//Gamming category
			RestCategoryChannel gammingCategory = await guild.CreateCategoryChannelAsync("Gamming", properties => properties.Position = 3);
			await gammingCategory.AddPermissionOverwriteAsync(guild.EveryoneRole, OverwritePermissions.InheritAll);
			await gammingCategory.AddPermissionOverwriteAsync(memberRole, OverwritePermissions.InheritAll);

			await guild.CreateTextChannelAsync("general", properties => properties.CategoryId = gammingCategory.Id);

			RestVoiceChannel gammingVcChannel = await AutoVCChannelCreator.CreateAutoVCChannel(guild, "Gamming");
			await gammingVcChannel.ModifyAsync(properties => properties.CategoryId = gammingCategory.Id);

			ServerListsManager.SaveServerList();
			await Context.Channel.SendMessageAsync("Quick Setup is done!");
		}
	}
}