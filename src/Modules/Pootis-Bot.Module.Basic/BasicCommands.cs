using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Config;
using Pootis_Bot.Core;

namespace Pootis_Bot.Module.Basic
{
	public sealed class BasicCommands : ModuleBase<SocketCommandContext>
	{
		private string displayName;

		public BasicCommands()
		{
			BotConfig config = Config<BotConfig>.Instance;
			displayName = config.BotName;
			config.Saved += () => displayName = config.BotName;
		}

		[Command("hello")]
		public async Task Hello()
		{
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("Hello!");
			embed.WithDescription($"Hello! My name is {displayName}!");
			embed.WithFooter($"Pootis-Bot: v{GetAppVersion()} - Discord.Net: v{GetDiscordNetVersion()}");
			embed.WithColor(new Color(241, 196, 15));

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}

		private static string GetAppVersion()
		{
			Assembly assembly = Assembly.GetEntryAssembly();
			if (assembly == null) return null;

			return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion;
		}

		public static string GetDiscordNetVersion()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			return (from assembly in assemblies
					where assembly.GetName().Name == "Discord.Net.Core"
					select assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion)
				.FirstOrDefault();
		}
	}
}