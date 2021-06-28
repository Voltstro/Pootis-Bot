using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.RuleReaction
{
    public class RuleReactionModule : Modules.Module
    {
        public override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("RuleReactionModule", "Voltstro", new Version(VersionUtils.GetCallingVersion()));
        }

        public override Task ClientConnected(DiscordSocketClient client)
        {
            client.ReactionAdded += (cacheable, channel, reaction) =>
            {
                _ = Task.Run(() => RuleReactionService.ReactionAdded(reaction, client));
                return Task.CompletedTask;
            };
            client.Ready += () =>
            {
                _ = Task.Run(() => RuleReactionService.CheckAllServer(client));
                return Task.CompletedTask;
            };
            client.RoleDeleted += RuleReactionService.RoleDeleted;
            client.ChannelDestroyed += RuleReactionService.ChannelDeleted;
            client.MessageDeleted += RuleReactionService.MessageDeleted;
            
            return base.ClientConnected(client);
        }
    }
}