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
			client.LeftGuild += guildEvents.LeftServer;

			//Message events
			MessageEvents messageEvents = new MessageEvents();
			client.MessageDeleted += messageEvents.MessageDeleted;
			client.MessagesBulkDeleted += messageEvents.MessageBulkDeleted;

			//Reaction events
			ReactionEvents reactionEvents = new ReactionEvents();
			client.ReactionAdded += reactionEvents.ReactionAdded;

			//Role events
			RoleEvents roleEvents = new RoleEvents();
			client.RoleDeleted += roleEvents.RoleDeleted;
			client.RoleUpdated += roleEvents.RoleUpdated;

			//User Related Events
			UserEvents userEvents = new UserEvents(client);
			client.UserJoined += userEvents.UserJoined;
			client.UserLeft += userEvents.UserLeft;
			client.UserBanned += userEvents.UserBanned;
			client.UserVoiceStateUpdated += userEvents.UserVoiceStateUpdated;
		}
	}
}