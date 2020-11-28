using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using JetBrains.Annotations;

namespace Pootis_Bot.Helper
{
	public static class MessageUtils
	{
		/// <summary>
		///		Send an error message to a message channel
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		[PublicAPI]
		public static async Task<RestUserMessage> SendErrorMessageAsync(this ISocketMessageChannel channel, string message)
		{
			return await channel.SendMessageAsync($"❌ {message}");
		}
	}
}