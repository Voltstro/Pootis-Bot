using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Pootis_Bot.Helper;
using Pootis_Bot.Services;
using Pootis_Bot.Services.Server;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Models;
using Emoji = Pootis_Bot.Core.Discord.Emoji;

namespace Pootis_Bot.Modules;

[Group("setup", "Commands for server setup")]
public class SetupModule : InteractionModuleBase<SocketInteractionContext>
{
    [Group("rule-reaction", "Commands related to rule reactions")]
    public class RuleReactionSubCommandGroupModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PootisBotDbContext dbContext;
    
        public RuleReactionSubCommandGroupModule(PootisBotDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        [SlashCommand("message", "Gets or sets the message that rule reactions should occur on")]
        public async Task SetupRrMessage(SocketTextChannel channel, [Summary("messageId", "Discord ID of the message to look for a reaction on.")] string? messageId = null)
        {
            ulong validMessageId = 0;
            if (messageId != null)
            {
                //Convert to ulong
                bool valid = ulong.TryParse(messageId, out validMessageId);
                if (!valid)
                {
                    await RespondAsync("Message ID is not a valid integer!");
                    return;
                }
                
                //Validate message ID first
                IMessage? message = await channel.GetMessageAsync(validMessageId);
                if (message == null)
                {
                    await RespondAsync($"Message of ID **{messageId}** was not found in channel {channel.Mention}!");
                    return;
                }

                await RespondAsync($"Message of ID **{messageId}** in channel {channel.Mention} has been set as the rule reaction message.");
                return;
            }
            
            //Set channel and message
            Server server = dbContext.GetOrCreateServer(Context.Guild);
            server.RuleReactionChannelId = channel.Id;
            
            if(validMessageId != 0)
                server.RuleReactionMessageId = validMessageId;
            await dbContext.SaveChangesAsync();

            //Create modal
            ModalBuilder modelBuilder = new ModalBuilder()
                .WithTitle("Rule Reaction Message")
                .WithCustomId(ServerSetupBackgroundService.ServerSetupRuleReactionModalId)
                .AddTextInput("Message", ServerSetupBackgroundService.ServerSetupRuleReactionModalMessageId, TextInputStyle.Paragraph, "What message would you like to be placed in this channel and used for rule-reactions?");

            await Context.Interaction.RespondWithModalAsync(modelBuilder.Build());
        }

        [SlashCommand("role", "Gets or sets what role is given on reaction")]
        public async Task SetupRrRole(SocketRole? role = null)
        {
            Server server = dbContext.GetOrCreateServer(Context.Guild);

            //No role provided then get the current role and print
            if (role == null)
            {
                if (server.RuleReactionRoleId == null)
                {
                    await RespondAsync($"Currently no rule reaction role is set.");
                    return;
                }

                SocketRole? currentRole = Context.Guild.GetRole(server.RuleReactionRoleId.Value);
                await RespondAsync($"Currently the rule reaction role is **{currentRole.Name}**.");
                return;
            }
            
            server.RuleReactionRoleId = role.Id;
            await dbContext.SaveChangesAsync();
            
            await RespondAsync($"Rule reaction role was set to **{role.Name}**.");
        }

        [SlashCommand("emoji", "Gets or sets what emoji is required to be reacted with")]
        public async Task SetupRrEmoji(Emoji? emoji = null)
        {
            Server server = dbContext.GetOrCreateServer(Context.Guild);

            if (emoji == null)
            {
                if (server.RuleReactionEmoji == null)
                {
                    await RespondAsync($"Currently no rule reaction emoji is set.");
                    return;
                }

                await RespondAsync($"Currently the rule reaction emoji set to \"{server.RuleReactionEmoji}\".");
                return;
            }

            server.RuleReactionEmoji = emoji.ToString();
            await dbContext.SaveChangesAsync();
            
            await RespondAsync($"Rule reaction emoji was set to \"{emoji.ToString()}\".");
        }
    }
}