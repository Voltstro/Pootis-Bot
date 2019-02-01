using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules.Audio
{
    public class MusicModule : ModuleBase<ICommandContext>
    {
        private readonly AudioService _service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public MusicModule()
        {
            AudioService service = new AudioService();
            _service = service;
        }

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("join", RunMode = RunMode.Async)]
        [Summary("Joins in the current voice channel you are in")]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Speak)]
        public async Task JoinCmd()
        {
            if(!Config.bot.isAudioServiceEnabled)
            {
                await Context.Channel.SendMessageAsync(":musical_note: Sorry, but audio services are disabled. :disappointed:");
                return;
            }
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel, Context.Channel);
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves the current voice channel thats it in")]
        public async Task LeaveCmd()
        {
            if (!Config.bot.isAudioServiceEnabled)
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
            if (!Config.bot.isAudioServiceEnabled)
            {
                await Context.Channel.SendMessageAsync(":musical_note: Sorry, but audio services are disabled. :disappointed:");
                return;
            }

            if (string.IsNullOrWhiteSpace(song))
            {
                await Context.Channel.SendMessageAsync($":musical_note: You need to input a song name! \nE.G: `{Bot.botprefix}play Still Alive`");
                return;
            }

            await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
        }

        [Command("stop")]
        [Summary("Stops the current playing song")]
        [RequireBotPermission(GuildPermission.Speak)]
        public async Task StopCmd()
        {
            if (!Config.bot.isAudioServiceEnabled)
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
            if (!Config.bot.isAudioServiceEnabled)
            {
                await Context.Channel.SendMessageAsync(":musical_note: Sorry, but audio services are disabled. :disappointed:");
                return;
            }

            await _service.PauseAudio(Context.Guild, Context.Channel);
        }
    }
}
