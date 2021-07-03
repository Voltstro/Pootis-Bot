using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Profiles
{
	internal sealed class ProfilesModule : Modules.Module
	{
		private XpLevelManager xpLevelManager;
		
		public override ModuleInfo GetModuleInfo()
		{
			return new ModuleInfo("ProfilesModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
		}

		public override Task Init()
		{
			xpLevelManager = new XpLevelManager();
			return base.Init();
		}

		public override async Task ClientMessage(DiscordSocketClient client, SocketUserMessage message)
		{
			await xpLevelManager.HandelUserMessage(message);
		}
	}
}