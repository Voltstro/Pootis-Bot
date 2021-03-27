using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Helper;

namespace Pootis_Bot.Module.Profiles
{
	[Name("Profile Commands")]
	[Summary("Provides profile commands")]
	public class ProfileCommands : ModuleBase<SocketCommandContext>
	{
		private readonly ProfilesConfig profilesConfig;

		public ProfileCommands()
		{
			profilesConfig = Config<ProfilesConfig>.Instance;
		}

		[Command("profile")]
		[Summary("Gets a user's profile")]
		public async Task GetUserProfile([Remainder] SocketUser user)
		{
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

		[Command("profile")]
		[Summary("Gets your profile")]
		public async Task GetUserProfile()
		{
			await GetUserProfile(Context.User);
		}
	}
}