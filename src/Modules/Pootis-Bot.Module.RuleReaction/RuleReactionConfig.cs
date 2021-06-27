using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Config;
using Pootis_Bot.Module.RuleReaction.Entities;

namespace Pootis_Bot.Module.RuleReaction
{
    internal class RuleReactionConfig : Config<RuleReactionConfig>
    {
        public List<RuleReactionServer> RuleReactionServers { get; set; } = new List<RuleReactionServer>();
        
        public RuleReactionServer GetOrCreateRuleReactionServer(ulong guildId)
        {
            RuleReactionServer ruleReactionServer = RuleReactionServers.FirstOrDefault(x => x.GuildId == guildId);
            if (ruleReactionServer == null)
                ruleReactionServer = CreateReactionServer(guildId);

            return ruleReactionServer;
        }

        private RuleReactionServer CreateReactionServer(ulong guildId)
        {
            RuleReactionServer reactionServer = new RuleReactionServer
            {
                GuildId = guildId
            };
            RuleReactionServers.Add(reactionServer);
            Save();
            return reactionServer;
        }
    }
}