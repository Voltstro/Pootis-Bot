using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Module.Profiles
{
	internal class XpLevelManager
	{
		private readonly ProfilesConfig profilesConfig;
		private readonly List<UserLevelData> users;

		internal XpLevelManager()
		{
			profilesConfig = Config<ProfilesConfig>.Instance;
			users = new List<UserLevelData>();
		}

		internal Task HandelUserMessage(SocketMessage message)
		{
			SocketUser messageAuthor = message.Author;

			//We don't allow bots or web hooks
			if(messageAuthor.IsBot || messageAuthor.IsWebhook)
				return Task.CompletedTask;

			UserLevelData user = GetOrCreateUser(message);

			//Make sure the message isn't the same
			string userMessage = message.Content.ToLower();
			if (userMessage == user.LastMessage)
			{
				user.LastMessage = userMessage;
				return Task.CompletedTask;
			}

			//Check cooldown time
			if ((DateTime.UtcNow - user.LastMessageThatAddedXpTime).TotalSeconds <=
			    profilesConfig.XpGiveCooldown.TotalSeconds)
			{
				return Task.CompletedTask;
			}

			//Add XP to the user
			AddXpToUser(messageAuthor);
			user.LastMessage = userMessage;
			user.LastMessageThatAddedXpTime = DateTime.UtcNow;

			return Task.CompletedTask;
		}

		internal void AddXpToUser(SocketUser user)
		{
			//We don't allow bots or web hooks
			if(user.IsBot || user.IsWebhook)
				return;

			Profile profile = profilesConfig.GetOrCreateProfile(user);
			profile.Xp += profilesConfig.XpGiveAmount;
			profilesConfig.Save();
			Logger.Debug("Added {XpAmount} XP to user {UserId}", profilesConfig.XpGiveAmount, user.Id);
		}

		private UserLevelData GetOrCreateUser(SocketMessage message)
		{
			IEnumerable<UserLevelData> result = from a in users
				where a.UserId == message.Author.Id
				select a;

			UserLevelData user = result.FirstOrDefault() ?? CreateUser(message.Author.Id, message.Content.ToLower());
			return user;
		}

		private UserLevelData CreateUser(ulong id, string message)
		{
			UserLevelData user = new UserLevelData
			{
				UserId = id,
				LastMessage = message,
				LastMessageThatAddedXpTime = DateTime.UtcNow
			};
			users.Add(user);
			return user;
		}

		private class UserLevelData
		{
			public ulong UserId;
			public DateTime LastMessageThatAddedXpTime;
			public string LastMessage;
		}
	}
}