using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services.Audio.Music;
using Pootis_Bot.Services.Google.YouTube;

namespace Pootis_Bot.Modules.Audio
{
	public class Music : ModuleBase<ICommandContext>
	{
		// Module Information
		// Original Author  - Creepysin
		// Description      - To run audio commands
		// Contributors     - Creepysin, 

		private readonly MusicService service;

		public Music(YouTubeService searcher)
		{
			service = new MusicService(searcher);
		}

		[Command("join", RunMode = RunMode.Async)]
		[Summary("Joins in the current voice channel you are in")]
		[Cooldown(5)]
		[RequireBotPermission(GuildPermission.Connect)]
		[RequireBotPermission(GuildPermission.Speak)]
		public async Task JoinCmd()
		{
			if (!Config.bot.AudioSettings.AudioServicesEnabled) //Check to see if the audio service is enabled 
			{
				await Context.Channel.SendMessageAsync(
					":musical_note: Sorry, but audio services are disabled. :disappointed:");
				return;
			}

			await service.JoinAudio(Context.Guild, ((IVoiceState) Context.User).VoiceChannel, Context.Channel,
				Context.User);
		}

		[Command("leave", RunMode = RunMode.Async)]
		[Summary("Leaves the current voice channel that the bot is it in")]
		public async Task LeaveCmd()
		{
			if (!Config.bot.AudioSettings.AudioServicesEnabled) //Check to see if the audio service is enabled 
			{
				await Context.Channel.SendMessageAsync(
					":musical_note: Sorry, but audio services are disabled. :disappointed:");
				return;
			}

			await service.LeaveAudio(Context.Guild, Context.Channel, Context.User);
		}

		[Command("play", RunMode = RunMode.Async)]
		[Summary("Plays a song")]
		[Cooldown(5)]
		[RequireBotPermission(GuildPermission.Speak)]
		public async Task PlayCmd([Remainder] string song = "")
		{
			if (!Config.bot.AudioSettings.AudioServicesEnabled) //Check to see if the audio service is enabled 
			{
				await Context.Channel.SendMessageAsync(
					":musical_note: Sorry, but audio services are disabled. :disappointed:");
				return;
			}

			await service.SendAudio((SocketGuild) Context.Guild, Context.Channel,
				((IVoiceState) Context.User).VoiceChannel, Context.User,
				song);
		}

		[Command("stop", RunMode = RunMode.Async)]
		[Summary("Stops the current playing song")]
		[RequireBotPermission(GuildPermission.Speak)]
		public async Task StopCmd()
		{
			if (!Config.bot.AudioSettings.AudioServicesEnabled) //Check to see if the audio service is enabled 
			{
				await Context.Channel.SendMessageAsync(
					":musical_note: Sorry, but audio services are disabled. :disappointed:");
				return;
			}

			await service.StopAudio(Context.Guild, Context.Channel, Context.User);
		}

		[Command("pause", RunMode = RunMode.Async)]
		[Summary("Pauses the current song")]
		[RequireBotPermission(GuildPermission.Speak)]
		public async Task PauseCmd()
		{
			if (!Config.bot.AudioSettings.AudioServicesEnabled) //Check to see if the audio service is enabled 
			{
				await Context.Channel.SendMessageAsync(
					":musical_note: Sorry, but audio services are disabled. :disappointed:");
				return;
			}

			await service.PauseAudio(Context.Guild, Context.Channel, Context.User);
		}
	}
}