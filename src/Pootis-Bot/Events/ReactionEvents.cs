using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services.Voting;

namespace Pootis_Bot.Events
{
	/// <summary>
	/// Handles reaction client events
	/// </summary>
	public class ReactionEvents
	{
		public Task ReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel,
			SocketReaction reaction)
		{
			SocketGuildUser user = (SocketGuildUser) reaction.User;

			//Make sure user isn't null
			if (user == null)
				return Task.CompletedTask;

			//Make sure the user isn't a bot as well
			if(user.IsBot)
				return Task.CompletedTask;

			//Something happened, make sure the message ID isn't 0, this is just in case
			if(reaction.MessageId == 0)
				return Task.CompletedTask;

			SocketGuild guild = ((SocketGuildChannel) channel).Guild;
			ServerList server = ServerListsManager.GetServer(guild);

			//If the message the user reacted to is the rules message
			if (reaction.MessageId == server.RuleMessageId && server.RuleEnabled)
			{
				if (reaction.Emote.Name != server.RuleReactionEmoji) //Check to make sure it is the right emoji
					return Task.CompletedTask;

				//Add the role
				SocketRole role = RoleUtils.GetGuildRole(guild, server.RuleRoleId);
				user.AddRoleAsync(role);

				return Task.CompletedTask;
			}

			//If this message is a vote
			if (server.GetVote(reaction.MessageId) != null)
			{
				Vote vote = server.GetVote(reaction.MessageId);
				if (reaction.Emote.Name == vote.YesEmoji)
					vote.YesCount++;
				else if (reaction.Emote.Name == vote.NoEmoji)
					vote.NoCount++;

				ServerListsManager.SaveServerList();

				return Task.CompletedTask;
			}

			//So the reaction wasn't anything important, so add some XP to the user
			LevelingSystem.GiveUserXp(user, (SocketTextChannel) reaction.Channel, 5);

			return Task.CompletedTask;
		}
	}
}