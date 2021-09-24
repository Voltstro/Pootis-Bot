using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Newtonsoft.Json;
using Pootis_Bot.Config;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Module.Profiles
{
	public class ProfilesConfig : Config<ProfilesConfig>
	{
		public uint XpGiveAmount { get; set; } = 15;

		public TimeSpan XpGiveCooldown { get; set; } = new TimeSpan(0, 0, 15);

		[JsonProperty("Profiles")]
		private List<Profile> profiles = new List<Profile>();

		/// <summary>
		///		Gets or creates a profile
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Profile GetOrCreateProfile(ulong id)
		{
			IEnumerable<Profile> result = from a in profiles
				where a.Id == id
				select a;

			Profile profile = result.FirstOrDefault() ?? CreateProfile(id);
			return profile;
		}

		/// <summary>
		///		Gets or creates a profile
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public Profile GetOrCreateProfile(SocketUser user)
		{
			return GetOrCreateProfile(user.Id);
		}

		/// <summary>
		///		Gets all <see cref="Profile"/>s
		/// </summary>
		/// <returns></returns>
		public Profile[] GetAllProfiles() => profiles.ToArray();

		private Profile CreateProfile(ulong id)
		{
			Profile profile = new Profile(id);
			profiles.Add(profile);
			Logger.Debug("Created new profile for {ProfileID}", id);
			Save();
			return profile;
		}
	}
}