using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services
{
	public static class ReminderService
	{
		/// <summary>
		/// Reminds a user in <paramref name="time"/> seconds of their message
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="time"></param>
		/// <param name="msg"></param>
		/// <returns></returns>
		public static async Task RemindAsyncSeconds(SocketUser guild, int time, string msg)
		{
			int convert = (int) TimeSpan.FromSeconds(time).TotalMilliseconds;
			string timenow = Global.TimeNow();

			await Task.Delay(convert);

			IDMChannel dm = await guild.CreateDMChannelAsync();

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("Reminder");
			embed.WithDescription(msg);
			embed.WithFooter($"Reminder was set at {timenow}", guild.GetAvatarUrl());

			await dm.SendMessageAsync("", false, embed.Build());
		}
	}
}