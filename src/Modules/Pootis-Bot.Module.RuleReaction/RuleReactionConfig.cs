using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Config;
using Pootis_Bot.Module.RuleReaction.Entities;

namespace Pootis_Bot.Module.RuleReaction
{
    /// <summary>
    ///     Rule reaction config
    /// </summary>
    public class RuleReactionConfig : Config<RuleReactionConfig>
    {
        public List<RuleReactionServer> RuleReactionServers { get; private set; } = new List<RuleReactionServer>();
        
        /// <summary>
        ///     Gets or creates a <see cref="RuleReactionServer"/>
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
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