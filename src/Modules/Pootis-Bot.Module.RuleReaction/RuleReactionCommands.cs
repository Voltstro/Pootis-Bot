using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Logging;
using Pootis_Bot.Module.RuleReaction.Entities;
using Emoji = Pootis_Bot.Discord.Emoji;

namespace Pootis_Bot.Module.RuleReaction
{
    [Group("rulereaction")]
    [Name("Rule Reaction")]
    [Summary("Provides rule reaction configuration commands")]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public class RuleReactionCommands : ModuleBase<SocketCommandContext>
    {
        private readonly RuleReactionConfig config;
        
        public RuleReactionCommands()
        {
            config = Config<RuleReactionConfig>.Instance;
        }
        
        [Command("emoji")]
        public async Task SetEmoji(Emoji emoji)
        {
            config.GetOrCreateRuleReactionServer(Context.Guild.Id).Emoji = emoji.Name;
            config.Save();
            await Context.Channel.SendMessageAsync($"Rule reaction emoji was set to {emoji}");
        }

        [Command("message")]
        [Alias("msg")]
        public async Task SetMessageCommand(ulong messageId)
        {
            await SetMessage(Context.Channel, messageId, Context.Channel, Context.Guild);
        }

        [Command("message")]
        [Alias("msg")]
        public async Task SetMessageCommand(SocketTextChannel channel, ulong messageId)
        {
            await SetMessage(channel, messageId, Context.Channel, Context.Guild);
        }

        [Command("role")]
        public async Task SetRole(SocketRole role)
        {
            config.GetOrCreateRuleReactionServer(Context.Guild.Id).RoleId = role.Id;
            config.Save();

            await Context.Channel.SendMessageAsync($"Rule reaction role was set to {role.Name}");
        }

        [Command("enable")]
        public async Task EnableRuleReaction()
        {
            RuleReactionServer server = config.GetOrCreateRuleReactionServer(Context.Guild.Id);
            if (!await RuleReactionService.CheckServer(server, Context.Client))
            {
                await Context.Channel.SendMessageAsync("The rule reaction is not setup correctly!");
                return;
            }

            server.Enabled = true;
            config.Save();
            await Context.Channel.SendMessageAsync("Rule reaction is now enabled.");
        }

        [Command("disable")]
        public async Task DisableRuleReaction()
        {
            RuleReactionServer server = config.GetOrCreateRuleReactionServer(Context.Guild.Id);
            server.Enabled = false;
            config.Save();
            await Context.Channel.SendMessageAsync("Rule reaction is now disabled.");
            
        }

        private async Task SetMessage(IMessageChannel channel, ulong messageId, ISocketMessageChannel messageChannel, SocketGuild guild)
        {
            //That message doesn't exist
            IMessage message = await channel.GetMessageAsync(messageId);
            if (message == null)
            {
                await messageChannel.SendMessageAsync("That message doesn't exist!");
                return;
            }

            RuleReactionServer reactionServer = config.GetOrCreateRuleReactionServer(guild.Id);
            reactionServer.ChannelId = channel.Id;
            reactionServer.MessageId = message.Id;
            config.Save();
            
            Logger.Debug("Guild {GuildId} set their rule reaction message to {MessageId}.", guild.Id, message.Id);
            await messageChannel.SendMessageAsync("The rule reaction message is set!");
        }
    }
}