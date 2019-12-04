using System.Collections.Generic;
using System.Linq;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core.Managers
{
	public class HighLevelProfileMessageManager
	{
		private const string HighLevelProfileMessagesFilePath = "Resources/HighLevelProfileMessages.json";
		public static List<HighLevelProfileMessage> HighLevelProfileMessages;

		static HighLevelProfileMessageManager()
		{
			if (DataStorage.SaveExists(HighLevelProfileMessagesFilePath))
			{
				HighLevelProfileMessages =
					DataStorage.LoadHighLevelProfileMessages(HighLevelProfileMessagesFilePath).ToList();
			}
			else
			{
				HighLevelProfileMessages = new List<HighLevelProfileMessage>
				{
					new HighLevelProfileMessage {UserId = 373808840568864768, Message = "Creator of Pootis-Bot"}
				};

				SaveHighLevelProfileMessages();
			}
		}

		/// <summary>
		///     Wow, it saves all the high level profile messages, who would of guess?
		/// </summary>
		public static void SaveHighLevelProfileMessages()
		{
			DataStorage.SaveHighLevelProfileMessages(HighLevelProfileMessages, HighLevelProfileMessagesFilePath);
		}

		/// <summary>
		///     Adds a high level profile message for a user
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="message"></param>
		public static void AddCustomHighLevelProfileMessage(ulong userId, string message)
		{
			HighLevelProfileMessages.Add(new HighLevelProfileMessage
			{
				UserId = userId,
				Message = message
			});

			SaveHighLevelProfileMessages();
		}

		/// <summary>
		///     Get a high level profile message
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static HighLevelProfileMessage GetHighLevelProfileMessage(ulong userId)
		{
			IEnumerable<HighLevelProfileMessage> result = from a in HighLevelProfileMessages
				where a.UserId == userId
				select a;

			HighLevelProfileMessage message = result.FirstOrDefault();
			return message;
		}
	}
}