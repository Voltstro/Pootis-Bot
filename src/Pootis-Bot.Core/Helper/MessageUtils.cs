using System;
using System.Threading.Tasks;
using Discord;
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

		/// <summary>
		///		Sends an embed using <see cref="EmbedBuilder"/>. The <see cref="Embed"/> will be built for you.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="embed"></param>
		/// <returns></returns>
		[PublicAPI]
		public static async Task<RestUserMessage> SendEmbedAsync(this ISocketMessageChannel channel, [NotNull] EmbedBuilder embed)
		{
			if(embed == null)
				throw new ArgumentNullException(nameof(embed));

			return await SendEmbedAsync(channel, embed.Build());
		}

		/// <summary>
		///		Sends an embed using <see cref="Embed"/>
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="embed"></param>
		/// <returns></returns>
		[PublicAPI]
		public static async Task<RestUserMessage> SendEmbedAsync(this ISocketMessageChannel channel, [NotNull] Embed embed)
		{
			if(embed == null)
				throw new ArgumentNullException(nameof(embed));

			return await channel.SendMessageAsync("", false, embed);
		}
	}
}