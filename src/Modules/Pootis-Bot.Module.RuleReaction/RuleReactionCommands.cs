using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Config;
using Pootis_Bot.Helper;
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

        [Command("status")]
        [Summary("Status of the rule reaction")]
        public async Task Status()
        {
            RuleReactionServer server = config.GetOrCreateRuleReactionServer(Context.Guild.Id);

            IMessage message = null;
            SocketTextChannel channel = Context.Guild.GetTextChannel(server.ChannelId);
            if (channel != null)
                message = await channel.GetMessageAsync(server.MessageId);
            
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Rule Reaction Status");
            embedBuilder.WithDescription($"Status of Rule Reaction for **{Context.Guild.Name}**");
            embedBuilder.AddField("Enabled?", server.Enabled);
            embedBuilder.AddField("Message", message == null ? "No Message" : $"[Link]({message.GetMessageUrl()})");
            embedBuilder.AddField("Emoji", string.IsNullOrEmpty(server.Emoji) ? "No Emoji" : server.Emoji);
            await Context.Channel.SendEmbedAsync(embedBuilder);
        }
        
        [Command("emoji")]
        [Summary("Sets the emoji to use for reactions")]
        public async Task SetEmoji(Emoji emoji)
        {
            config.GetOrCreateRuleReactionServer(Context.Guild.Id).Emoji = emoji.Name;
            config.Save();
            await Context.Channel.SendMessageAsync($"Rule reaction emoji was set to {emoji}");
        }

        [Command("message")]
        [Alias("msg")]
        [Summary("Sets what message to use (NOTE: The message needs to be in the same channel where the command is executed)")]
        public async Task SetMessageCommand(ulong messageId)
        {
            await SetMessage(Context.Channel, messageId, Context.Channel, Context.Guild);
        }

        [Command("message")]
        [Alias("msg")]
        [Summary("Sets what message to use (Spefiy channel as to where the message is)")]
        public async Task SetMessageCommand(SocketTextChannel channel, ulong messageId)
        {
            await SetMessage(channel, messageId, Context.Channel, Context.Guild);
        }

        [Command("role")]
        [Summary("Sets what role will be given on reaction")]
        public async Task SetRole(SocketRole role)
        {
            config.GetOrCreateRuleReactionServer(Context.Guild.Id).RoleId = role.Id;
            config.Save();

            await Context.Channel.SendMessageAsync($"Rule reaction role was set to {role.Name}");
        }

        [Command("enable")]
        [Summary("Verifys everything is ready to go and enables rule reaction")]
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
        [Summary("Disables rule reaction")]
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