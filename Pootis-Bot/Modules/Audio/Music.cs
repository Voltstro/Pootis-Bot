using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Services;

namespace Pootis_Bot.Modules.Audio
{
    public class Music : ModuleBase<ICommandContext>
    {
        // Module Infomation
        // Orginal Author   - Creepysin
        // Description      - To run audio commands
        // Contributors     - Creepysin, 

        private readonly AudioService _service;

        public Music()
        {
            _service = new AudioService();
        }

        [Command("join", RunMode = RunMode.Async)]
        [Summary("Joins in the current voice channel you are in")]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Speak)]
        public async Task JoinCmd()
        {
            if(!Config.bot.isAudioServiceEnabled) //Check to see if the audio service is enabled 
            {
                await Context.Channel.SendMessageAsync(":musical_note: Sorry, but audio services are disabled. :disappointed:");
                return;
            }

            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel, Context.Channel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves the current voice channel thats it in")]
        public async Task LeaveCmd()
        {
            if (!Config.bot.isAudioServiceEnabled) //Check to see if the audio service is enabled 
            {
                await Context.Channel.SendMessageAsync(":musical_note: Sorry, but audio services are disabled. :disappointed:");
                return;
            }

            await _service.LeaveAudio(Context.Guild, Context.Channel);
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Plays a song")]
        [RequireBotPermission(GuildPermission.Speak)]
        public async Task PlayCmd([Remainder] string song = "")
        {
            if (!Config.bot.isAudioServiceEnabled) //Check to see if the audio service is enabled 
            {
                await Context.Channel.SendMessageAsync(":musical_note: Sorry, but audio services are disabled. :disappointed:");
                return;
            }

            await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Summary("Stops the current playing song")]
        [RequireBotPermission(GuildPermission.Speak)]
        public async Task StopCmd()
        {
            if (!Config.bot.isAudioServiceEnabled) //Check to see if the audio service is enabled 
            {
                await Context.Channel.SendMessageAsync(":musical_note: Sorry, but audio services are disabled. :disappointed:");
                return;
            }

            await _service.StopAudioAsync(Context.Guild, Context.Channel);
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Summary("Pauses the current song")]
        [RequireBotPermission(GuildPermission.Speak)]
        public async Task PauseCmd()
        {
            if (!Config.bot.isAudioServiceEnabled) //Check to see if the audio service is enabled 
            {
                await Context.Channel.SendMessageAsync(":musical_note: Sorry, but audio services are disabled. :disappointed:");
                return;
            }

            await _service.PauseAudio(Context.Guild, Context.Channel);
        }
    }
}
