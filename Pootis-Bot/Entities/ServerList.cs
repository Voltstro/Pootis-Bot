using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Entities
{
	public class ServerList
	{
		/// <summary>
		/// The id of the guild
		/// </summary>
		public ulong GuildId { get; set; }

		/// <summary>
		/// Channels were the bot isn't allowed to except commands from
		/// </summary>
		public List<ulong> BannedChannels { get; set; }

		/// <summary>
		/// Command permissions
		/// </summary>
		public List<CommandInfo> CommandInfos { get; set; }

		/// <summary>
		/// Role to role mentions
		/// </summary>
		public List<RoleToRoleMention> RoleToRoleMentions { get; set; }

		/// <summary>
		/// Auto voice channels
		/// </summary>
		public List<VoiceChannel> AutoVoiceChannels { get; set; }

		/// <summary>
		/// Any active channels that were created from an auto-vc channel
		/// </summary>
		public List<ulong> ActiveAutoVoiceChannels { get; set; }

		/// <summary>
		/// Do we have custom welcome/goodbye messages enabled
		/// </summary>
		public bool WelcomeMessageEnabled { get; set; }

		/// <summary>
		/// What is the channel id were we put the messages
		/// </summary>
		public ulong WelcomeChannelId { get; set; }

		/// <summary>
		/// The welcome message
		/// </summary>
		public string WelcomeMessage { get; set; }

		/// <summary>
		/// The goodbye message
		/// </summary>
		public string WelcomeGoodbyeMessage { get; set; }

		/// <summary>
		/// Is the rule reaction feature enabled?
		/// </summary>
		public bool RuleEnabled { get; set; }

		/// <summary>
		/// What is the message that needs to be reacted
		/// </summary>
		public ulong RuleMessageId { get; set; }

		/// <summary>
		/// The role that will be given to the user after reacting with the right emoji
		/// </summary>
		public ulong RuleRoleId { get; set; }

		/// <summary>
		/// The emoji that needs to be used
		/// </summary>
		public string RuleReactionEmoji { get; set; }

		/// <summary>
		/// Anti-spam settings
		/// </summary>
		public AntiSpamSettingsInfo AntiSpamSettings { get; set; }

		public class CommandInfo
		{
			/// <summary>
			/// The name of the command
			/// </summary>
			public string Command { get; set; }

			/// <summary>
			/// Roles that are allowed to use the command
			/// </summary>
			public List<string> Roles { get; set; }
		}

		public class AntiSpamSettingsInfo
		{
			/// <summary>
			/// Is the mass user anti-spam feature enabled?
			/// </summary>
			public bool MentionUserEnabled { get; set; }

			/// <summary>
			/// What is the threshold of how many users can be mentioned
			/// </summary>
			public int MentionUsersPercentage { get; set; }

			/// <summary>
			/// How many times can a user violate a role to role mention before warnings are given out
			/// </summary>
			public int RoleToRoleMentionWarnings { get; set; }
		}

		#region Functions

		/// <summary>
		/// Gets a banned channel
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ulong GetBannedChannel(ulong id)
		{
			IEnumerable<ulong> result = from a in BannedChannels
				where a == id
				select a;

			ulong channel = result.FirstOrDefault();
			if (channel == 0) channel = CreateBannedChannel(id);
			return channel;
		}

		/// <summary>
		/// Creates a banned channel
		/// </summary>
		/// <param name="channelId"></param>
		/// <returns></returns>
		public ulong CreateBannedChannel(ulong channelId)
		{
			BannedChannels.Add(channelId);
			return channelId;
		}

		/// <summary>
		/// Gets an auto voice channel
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public VoiceChannel GetAutoVoiceChannel(ulong id)
		{
			IEnumerable<VoiceChannel> result = from a in AutoVoiceChannels
				where a.Id == id
				select a;

			VoiceChannel channel = result.FirstOrDefault();
			return channel;
		}

		/// <summary>
		/// Get an active auto voice channel
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ulong GetActiveVoiceChannel(ulong id)
		{
			IEnumerable<ulong> result = from a in ActiveAutoVoiceChannels
				where a == id
				select a;

			ulong channel = result.FirstOrDefault();
			if (channel == 0) channel = 0;
			return channel;
		}

		/// <summary>
		/// Get an command permission
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public CommandInfo GetCommandInfo(string command)
		{
			IEnumerable<CommandInfo> result = from a in CommandInfos
				where a.Command == command
				select a;

			CommandInfo commandInfo = result.FirstOrDefault();
			return commandInfo;
		}

		/// <summary>
		/// Get a role to role ping
		/// </summary>
		/// <param name="roleId"></param>
		/// <returns></returns>
		public List<RoleToRoleMention> GetRoleToRoleMention(ulong roleId)
		{
			IEnumerable<RoleToRoleMention> result = from a in RoleToRoleMentions
				where a.RoleId == roleId
				select a;


			List<RoleToRoleMention> roleToRoleMention = result.ToList();
			return roleToRoleMention;
		}

		/// <summary>
		/// Create a role to role ping
		/// </summary>
		/// <param name="roleNotMention"></param>
		/// <param name="role"></param>
		/// <returns></returns>
		public RoleToRoleMention CreateRoleToRoleMention(ulong roleNotMention, ulong role)
		{
			RoleToRoleMention roleToRole = new RoleToRoleMention(roleNotMention, role);
			RoleToRoleMentions.Add(roleToRole);
			return roleToRole;
		}

		#endregion
	}
}