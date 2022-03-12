using System.Threading.Tasks;
using Cysharp.Text;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Pootis_Bot.Config;

namespace Pootis_Bot.Module.AutoVC;

[Group("autovc", "Provides commands for auto voice channels")]
public class AutoVCCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly AutoVCConfig config;

    public AutoVCCommands()
    {
        config = Config<AutoVCConfig>.Instance;
    }

    [SlashCommand("add", "Adds an auto voice channel")]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.MoveMembers)]
    public async Task AddAutoVC(string baseName)
    {
        RestVoiceChannel voiceChannel =
            await Context.Guild.CreateVoiceChannelAsync(ZString.Format(config.BaseName, baseName));
        config.AddAutoVc(voiceChannel.Id, voiceChannel.GuildId, baseName);
        config.Save();

        await RespondAsync("Added AutoVC to the server.");
    }
}