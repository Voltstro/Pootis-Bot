using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Core;
using Pootis_Bot.Helper;

namespace Pootis_Bot.Module.Profiles
{
	[Group("profile")]
	[Name("Profile Commands")]
	[Summary("Provides profile commands")]
	public class ProfileCommands : ModuleBase<SocketCommandContext>
	{
		private readonly ProfilesConfig profilesConfig;
		private string displayName;

		public ProfileCommands()
		{
			profilesConfig = Config<ProfilesConfig>.Instance;
			BotConfig config = Config<BotConfig>.Instance;
			displayName = config.BotName;
			config.Saved += () => displayName = config.BotName;
		}

		[Command]
		[Summary("Gets a user's profile")]
		public async Task GetUserProfile([Remainder] SocketUser user)
		{
			if (user.IsBot || user.IsWebhook)
				return;

			Profile profile = profilesConfig.GetOrCreateProfile(user);

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle($"{user.Username}'s Profile");
			embed.WithFooter(profile.UserProfileMessage, user.GetAvatarUrl());
			embed.WithCurrentTimestamp();
			embed.WithThumbnailUrl(user.GetAvatarUrl(ImageFormat.Auto, 256));
			embed.AddField("Stats", $"**Level**: {profile.LevelNumber}\n**Xp**: {profile.Xp}\n", true);
			embed.AddField("Account",
				$"**Id**: {user.Id}\n**Creation Date**: {user.CreatedAt.DateTime.ToUniversalTime():yyyy MMMM dd h:mm tt UTC}");

			await Context.Channel.SendEmbedAsync(embed);
		}

		[Command]
		[Summary("Gets your profile")]
		public async Task GetUserProfile()
		{
			await GetUserProfile(Context.User);
		}

		[Command("message")]
		[Summary("Sets your user message")]
		public async Task SetUserProfileMessage([Remainder] string message)
		{
			if (message == null)
			{
				await Context.Channel.SendErrorMessageAsync("Your message cannot be null!");
				return;
			}

			Profile profile = profilesConfig.GetOrCreateProfile(Context.User);
			profile.UserProfileMessage = message;
			profilesConfig.Save();

			await Context.Channel.SendMessageAsync($"Your profile message was updated to '{message}'");
		}

		[Command("top10")]
		[Summary("Gets the top 10 profiles")]
		public async Task GetTop10Profiles()
		{
			Profile[] allProfiles = profilesConfig.GetAllProfiles();
			Array.Sort(allProfiles, new SortProfiles());
			//Array.Reverse(allProfiles);

			Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();
			sb.Append($"```csharp\n 📋 Top 10 {displayName} Profiles\n ========================\n");
			int count = 1;
			foreach (Profile user in allProfiles.Where(_ => count <= 10))
			{
				SocketUser targetUser = Context.Client.GetUser(user.Id);

				sb.Append(
					$"\n [{count}] -- # {targetUser.Username}\n         └ Level: {user.LevelNumber}\n         └ Xp: {user.Xp}");
				count++;
			}

			Profile callersProfile = profilesConfig.GetOrCreateProfile(Context.User);
			sb.Append(
				$"\n------------------------\n 😊 {Context.User.Username}'s Position: {Array.IndexOf(allProfiles, callersProfile) + 1}      {Context.User.Username}'s Level: {callersProfile.LevelNumber}      {Context.User.Username}'s Xp: {callersProfile.Xp}```");

			await Context.Channel.SendMessageAsync(sb.ToString());
			sb.Dispose();
		}

		private class SortProfiles : IComparer<Profile>
		{
			public int Compare(Profile x, Profile y)
			{
				if (y != null && x != null && x.Xp < y.Xp)
					return 1;
				if (y != null && x != null && x.Xp > y.Xp)
					return -1;
				return 0;
			}
		}
	}
}