using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Entities
{
	public class GlobalServerList
	{
		public List<ulong> ActiveAutoVoiceChannels = new List<ulong>();

		public List<ulong> BannedChannels = new List<ulong>();

		public List<CommandInfo> CommandInfos = new List<CommandInfo>();

		public List<RoleToRoleMention> RoleToRoleMentions = new List<RoleToRoleMention>();

		public List<VoiceChannel> VoiceChannels = new List<VoiceChannel>();
		public ulong ServerId { get; set; }

		public ulong WelcomeChannel { get; set; }
		public bool WelcomeMessageEnabled { get; set; }
		public string WelcomeMessage { get; set; }
		public string WelcomeGoodbyeMessage { get; set; }

		public bool RuleEnabled { get; set; }
		public ulong RuleMessageId { get; set; }
		public ulong RuleRoleId { get; set; }
		public string RuleReactionEmoji { get; set; }

		public AntiSpamSettingsInfo AntiSpamSettings { get; set; }

		public class CommandInfo
		{
			public List<string> Roles = new List<string>();
			public string Command { get; set; }
		}

		public class AntiSpamSettingsInfo
		{
			/// <summary>
			/// Are we allowed to mention users?
			/// </summary>
			public bool MentionUserEnabled { get; set; }

			/// <summary>
			/// How much of our messsage can be mentions?
			/// </summary>
			public int MentionUsersPercentage { get; set; }

			public int RoleToRoleMentionWarnings { get; set; }
		}

		#region Functions

		public ulong GetOrCreateBannedChannel(ulong id)
		{
			IEnumerable<ulong> result = from a in BannedChannels
				where a == id
				select a;

			ulong channel = result.FirstOrDefault();
			if (channel == 0) channel = CreateBannedChannel(id);
			return channel;
		}

		public VoiceChannel GetVoiceChannel(ulong id)
		{
			IEnumerable<VoiceChannel> result = from a in VoiceChannels
				where a.Id == id
				select a;

			VoiceChannel channel = result.FirstOrDefault();
			return channel;
		}

		public ulong GetActiveVoiceChannel(ulong id)
		{
			IEnumerable<ulong> result = from a in ActiveAutoVoiceChannels
				where a == id
				select a;

			ulong channel = result.FirstOrDefault();
			if (channel == 0) channel = 0;
			return channel;
		}

		public CommandInfo GetCommandInfo(string command)
		{
			IEnumerable<CommandInfo> result = from a in CommandInfos
				where a.Command == command
				select a;

			CommandInfo commandInfo = result.FirstOrDefault();
			return commandInfo;
		}

		private ulong CreateBannedChannel(ulong channelId)
		{
			BannedChannels.Add(channelId);
			return channelId;
		}

		public List<RoleToRoleMention> GetRoleToRoleMention(ulong roleId)
		{
			IEnumerable<RoleToRoleMention> result = from a in RoleToRoleMentions
				where a.RoleId == roleId
				select a;


			List<RoleToRoleMention> roleToRoleMention = result.ToList();
			return roleToRoleMention;
		}

		public RoleToRoleMention CreateRoleToRoleMention(ulong roleNotMention, ulong role)
		{
			RoleToRoleMention roleToRole = new RoleToRoleMention(roleNotMention, role);
			RoleToRoleMentions.Add(roleToRole);
			return roleToRole;
		}

		#endregion
	}
}