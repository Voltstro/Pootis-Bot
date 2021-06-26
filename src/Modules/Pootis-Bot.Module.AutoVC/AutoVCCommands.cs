using System.Threading.Tasks;
using Cysharp.Text;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Pootis_Bot.Config;

namespace Pootis_Bot.Module.AutoVC
{
    [Group("autovc")]
    [Name("Auto Voice Channels")]
    [Summary("Provides commands for auto voice channels")]
    public class AutoVCCommands : ModuleBase<SocketCommandContext>
    {
        private readonly AutoVCConfig config;
        
        public AutoVCCommands()
        {
            config = Config<AutoVCConfig>.Instance;
        }
        
        [Command("add")]
        [Summary("Adds an auto voice channel")]
        [RequireBotPermission(ChannelPermission.ManageChannels)]
        public async Task AddAutoVC(string baseName)
        {
            RestVoiceChannel voiceChannel = await Context.Guild.CreateVoiceChannelAsync(ZString.Format(config.BaseName, baseName));
            config.AddAutoVc(voiceChannel.Id, voiceChannel.GuildId, baseName);
            config.Save();
        }
    }
}