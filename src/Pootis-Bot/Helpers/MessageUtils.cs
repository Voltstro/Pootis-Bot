using System.Threading.Tasks;
using Discord;

namespace Pootis_Bot.Helpers
{
	public static class MessageUtils
	{
		/// <summary>
		/// Modifies a pre existing Discord message with a new message
		/// </summary>
		/// <param name="baseMessage"></param>
		/// <param name="newMessage"></param>
		/// <returns></returns>
		public static async Task ModifyMessage(IUserMessage baseMessage, string newMessage)
		{
			await baseMessage.ModifyAsync(x => { x.Content = newMessage; });
		}

		/// <summary>
		/// Modifies a pre existing Discord message with a new embed
		/// </summary>
		/// <param name="baseMessage"></param>
		/// <param name="embed"></param>
		/// <returns></returns>
		public static async Task ModifyMessage(IUserMessage baseMessage, EmbedBuilder embed)
		{
			await baseMessage.ModifyAsync(x => { x.Embed = embed.Build(); });
		}
	}
}