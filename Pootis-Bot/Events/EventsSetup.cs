using Discord.WebSocket;

namespace Pootis_Bot.Events
{
	public class EventsSetup
	{
		public EventsSetup(DiscordSocketClient client)
		{
			//Channel events
			ChannelEvents channelEvents = new ChannelEvents();
			client.ChannelDestroyed += channelEvents.ChannelDestroyed;

			//Guild events
			GuildEvents guildEvents = new GuildEvents();
			client.JoinedGuild += guildEvents.JoinedNewServer;

			//Reaction events
			ReactionEvents reactionEvents = new ReactionEvents();
			client.ReactionAdded += reactionEvents.ReactionAdded;

			//User Related Events
			UserEvents userEvents = new UserEvents(client);
			client.UserJoined += userEvents.UserJoined;
			client.UserLeft += userEvents.UserLeft;
			client.UserVoiceStateUpdated += userEvents.UserVoiceStateUpdated;
		}
	}
}
