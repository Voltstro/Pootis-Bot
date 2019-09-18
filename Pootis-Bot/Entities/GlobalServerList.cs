using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Entities
{
    public class GlobalServerList
    {
        public ulong ServerId { get; set; }

        public ulong WelcomeChannel { get; set; }
        public bool WelcomeMessageEnabled { get; set; }
        public string WelcomeMessage { get; set; }
        public string WelcomeGoodbyeMessage { get; set; }

        public bool RuleEnabled { get; set; }
        public ulong RuleMessageId { get; set; }
        public string RuleRole { get; set; }
        public string RuleReactionEmoji { get; set; }

		public AntiSpamSettingsInfo AntiSpamSettings { get; set; }

        public List<ulong> BannedChannels = new List<ulong>();

        public List<CommandInfo> CommandInfos = new List<CommandInfo>();

        public List<VoiceChannel> VoiceChannels = new List<VoiceChannel>();

        public List<ulong> ActiveAutoVoiceChannels = new List<ulong>();

		public List<RoleToRoleMention> RoleToRoleMentions = new List<RoleToRoleMention>();

		public class CommandInfo
        {
            public string Command { get; set; }
            public List<string> Roles = new List<string>();
        }

		public class AntiSpamSettingsInfo
		{
			public  bool MentionUserEnabled { get; set; }

			public int MentionUsersPercentage { get; set; }

			public int RoleToRoleMentionWarnings { get; set; }
		}

		#region Functions

		public ulong GetOrCreateBannedChannel(ulong id)
        {
            var result = from a in BannedChannels
                         where a == id
                         select a;

            var channel = result.FirstOrDefault();
            if (channel == 0) channel = CreateBannedChannel(id);
            return channel;
        }

        public VoiceChannel GetVoiceChannel(ulong id)
        {
            var result = from a in VoiceChannels
                         where a.Id == id
                         select a;

            var channel = result.FirstOrDefault();
            return channel;
        }

        public ulong GetActiveVoiceChannel(ulong id)
        {
            var result = from a in ActiveAutoVoiceChannels
                         where a == id
                         select a;

            var channel = result.FirstOrDefault();
            if (channel == 0) channel = 0;
            return channel;
        }

        public CommandInfo GetCommandInfo(string command)
        {
            var result = from a in CommandInfos
                         where a.Command == command
                         select a;

            var commandInfo = result.FirstOrDefault();
            return commandInfo;
        }

        private ulong CreateBannedChannel(ulong channelId)
        {
            BannedChannels.Add(channelId);
            return channelId;
        }

		public List<RoleToRoleMention> GetRoleToRoleMention(string role)
		{
			var result = from a in RoleToRoleMentions
						 where a.Role == role
						 select a;

			
			var roleToRoleMention = result.ToList();
			return roleToRoleMention;
		}

		public RoleToRoleMention CreateRoleToRoleMention(string roleNotMention, string role)
		{
			var roleToRole = new RoleToRoleMention(roleNotMention, role);
			RoleToRoleMentions.Add(roleToRole);
			return roleToRole;
		}

		#endregion
	}
}