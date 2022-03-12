﻿using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Module.WelcomeMessage.Entities;

namespace Pootis_Bot.Module.WelcomeMessage;

public class WelcomeMessageConfig : Config<WelcomeMessageConfig>
{
    public List<WelcomeMessageServer> WelcomeMessageServers { get; } = new();

    public WelcomeMessageServer GetOrCreateWelcomeMessageServer(SocketGuild guild)
    {
        return GetOrCreateWelcomeMessageServer(guild.Id);
    }

    public WelcomeMessageServer GetOrCreateWelcomeMessageServer(ulong guildId)
    {
        WelcomeMessageServer server = WelcomeMessageServers.FirstOrDefault(x => x.GuildId == guildId) ??
                                      CreateWelcomeMessageServer(guildId);
        return server;
    }

    private WelcomeMessageServer CreateWelcomeMessageServer(ulong guildId)
    {
        WelcomeMessageServer welcomeMessageServer = new(guildId);
        WelcomeMessageServers.Add(welcomeMessageServer);
        Save();
        return welcomeMessageServer;
    }
}