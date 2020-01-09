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
				if (reaction.MessageId != 0 && server.GetVote(reaction.MessageId) != null)
				{
					Vote vote = server.GetVote(reaction.MessageId);
					if (reaction.Emote.Name == vote.YesEmoji)
						vote.YesCount++;
					else if (reaction.Emote.Name == vote.NoEmoji)
						vote.NoCount++;
					ServerListsManager.SaveServerList();
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