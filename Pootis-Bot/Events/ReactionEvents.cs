using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;
using Pootis_Bot.Helpers;
using Pootis_Bot.Services;

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
			SocketGuild guild = ((SocketGuildChannel) channel).Guild;
			ServerList server = ServerListsManager.GetServer(guild);

			if (reaction.MessageId == server.RuleMessageId) //Check to see if the reaction is on the right message
			{
				if (!server.RuleEnabled) return Task.CompletedTask;
				if (reaction.Emote.Name != server.RuleReactionEmoji) return Task.CompletedTask;
				SocketRole role = RoleUtils.GetGuildRole(guild, server.RuleRoleId);

				SocketGuildUser user = (SocketGuildUser) reaction.User;
				user.AddRoleAsync(role);
			}
			else
			{
				if (VoteGiveawayService.IsVoteRunning
				) // If there is a vote going on then check to make sure the reaction doesn't have anything to do with that.
				{
					foreach (VoteGiveawayService.Vote vote in VoteGiveawayService.votes.Where(vote =>
						reaction.MessageId == vote.VoteMessageId))
						if (reaction.Emote.Name == vote.YesEmoji)
							vote.YesCount++;

						else if (reaction.Emote.Name == vote.NoEmoji) vote.NoCount++;
				}
				else
				{
					if (!((SocketGuildUser) reaction.User).IsBot)
						LevelingSystem.UserSentMessage((SocketGuildUser) reaction.User,
							(SocketTextChannel) reaction.Channel, 5);
				}
			}

			return Task.CompletedTask;
		}
	}
}
