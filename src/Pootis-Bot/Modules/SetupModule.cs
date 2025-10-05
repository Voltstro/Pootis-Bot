using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Helper;
using Pootis_Bot.Services;
using Pootis_Bot.Services.Interactions.Buttons;
using Pootis_Bot.Services.Interactions.Modal;
using Pootis_Bot.Services.Interactions.SelectMenu;
using Pootis_Bot.Services.Server;
using Pootis_Bot.Shared;
using Pootis_Bot.Shared.Helper;
using Pootis_Bot.Shared.Models;
using Emoji = Pootis_Bot.Core.Discord.Emoji;
using MessageType = Pootis_Bot.Shared.Messages.MessageType;

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

        [SlashCommand("status", "Gets the status of rule reaction setup")]
        public async Task SetupRrStatus()
        {
            Server server = dbContext.GetOrCreateServer(Context.Guild);
            
            IMessage? message = null;
            SocketTextChannel? channel = server.RuleReactionChannelId.HasValue ? Context.Guild.GetTextChannel(server.RuleReactionChannelId.Value) : null;
            if (channel != null && server.RuleReactionMessageId.HasValue)
                message = await channel.GetMessageAsync(server.RuleReactionMessageId.Value);
        
            //Role
            SocketRole? role = null;
            if(server.RuleReactionRoleId.HasValue)
                role = Context.Guild.GetRole(server.RuleReactionRoleId.Value);

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithTitle("Rule Reaction Status");
            embedBuilder.WithDescription($"Status of Rule Reaction for **{Context.Guild.Name}**");
            embedBuilder.AddField("Enabled?", server.RuleReactionEnabled);
            embedBuilder.AddField("Message", message == null ? "No Message Set" : $"[Link]({message.GetMessageUrl()})");
            embedBuilder.AddField("Emoji", string.IsNullOrEmpty(server.RuleReactionEmoji) ? "No Emoji Set" : server.RuleReactionEmoji);
            embedBuilder.AddField("Role", role == null ? "No Role Set" : role.Name);
        
            await RespondAsync(embed: embedBuilder.Build());
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

        [SlashCommand("toggle", "Enables/disables rule reaction")]
        public async Task SetupRrEnable()
        {
            Server server = dbContext.GetOrCreateServer(Context.Guild);

            //Need to check that everything is all good (if we are enabling)
            if (!server.RuleReactionEnabled)
            {
                if (server.RuleReactionChannelId == null || server.RuleReactionMessageId == null)
                {
                    await RespondAsync("A channel and message must be set before rule reaction can be enabled!");
                    return;
                }

                if (server.RuleReactionRoleId == null)
                {
                    await RespondAsync("A role must be set before rule reaction can be enabled!");
                    return;
                }

                if (server.RuleReactionEmoji == null)
                {
                    await RespondAsync("An emoji must be set before rule reaction can be enabled!");
                    return;
                }
            }
            
            server.RuleReactionEnabled = !server.RuleReactionEnabled;
            await dbContext.SaveChangesAsync();
            
            if(server.RuleReactionEnabled)
                await RespondAsync("Rule reaction are now enabled.");
            else
                await RespondAsync("Rule reaction are now disabled.");
        }
    }

    [Group("welcome-goodbye", "Commands related to welcome and goodbye messages")]
    public class WelcomeGoodbyeSubGroupModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PootisBotDbContext dbContext;

        private readonly ServerSetupService serverSetupService;
        private readonly ButtonsService buttonsService;
        private readonly SelectMenuService selectMenuService;
        private readonly ModalService modalService;
    
        public WelcomeGoodbyeSubGroupModule(
            PootisBotDbContext dbContext,
            ServerSetupService serverSetupService,
            ButtonsService buttonsService,
            SelectMenuService selectMenuService,
            ModalService modalService)
        {
            this.dbContext = dbContext;
            this.serverSetupService = serverSetupService;
            this.buttonsService = buttonsService;
            this.selectMenuService = selectMenuService;
            this.modalService = modalService;
        }

        [SlashCommand("channel", "Gets or sets the channel used for welcome and goodbye messages")]
        public async Task SetupWgChannel(SocketTextChannel? channel = null)
        {
            Server server = dbContext.GetOrCreateServer(Context.Guild);

            if (channel == null)
            {
                if (server.WelcomeGoodbyeChannelId == null)
                {
                    await RespondAsync("No welcome/goodbye channel is set.");
                    return;
                }
                
                channel = Context.Guild.GetTextChannel(server.WelcomeGoodbyeChannelId.Value);
                await RespondAsync($"Welcome and goodbye channel is currently set to {channel.Mention}");
                return;
            }

            server.WelcomeGoodbyeChannelId = channel.Id;
            await dbContext.SaveChangesAsync();

            await RespondAsync($"Welcome/goodbye channel is now set to {channel.Mention}");
        }

        [SlashCommand("messages", "Allows setup of messages")]
        public async Task SetupWgMessages(MessageType messageType)
        {
            SocketGuild guild = Context.Guild;
            Server server = dbContext.GetOrCreateServer(guild);
            ServerMessage[] messages = await dbContext.ServerMessages
                .Where(x => x.ServerId == server.Id && x.Type == messageType)
                .ToArrayAsync();
            
            //Embed
            EmbedBuilder embedBuilder = new();
            embedBuilder.WithTitle($"All {messageType.ToString()} messages");
            
            StringBuilder sb = new();
            if (messages.Length > 0)
            {
                sb.AppendLine($"Total of {messages.Length} messages:");
                foreach (ServerMessage serverMessage in messages)
                {
                    sb.AppendLine($" - {serverMessage.Message}");
                }
            }
            else
            {
                sb.AppendLine("No messages available.");
            }
            embedBuilder.WithDescription(sb.ToString());
            
            //Buttons
            Dictionary<string, string> options = new();
            foreach (ServerMessage serverMessage in messages)
            {
                options.Add(serverMessage.Message, serverMessage.Id.ToString());
            }
            
            List<Button> buttons =
            [
                new Button
                {
                    Label = "Add New Message",
                    Style = ButtonStyle.Primary,
                    DisableOnClick = true,
                    Action = async messageComponent =>
                    {
                        Modal modal = modalService.CreateModal($"Enter New {messageType} Message", async (SetupWgAddMessage addMessage, SocketModal socketModal) =>
                        {
                            if (!socketModal.GuildId.HasValue)
                                return;
                            
                            await serverSetupService.AddMessage(socketModal.GuildId.Value, messageType, addMessage.Message);
                            await socketModal.RespondAsync($"New {messageType.ToString().ToLower()} message has been saved.");
                        });

                        await messageComponent.RespondWithModalAsync(modal);
                    }
                },
                new Button
                {
                    Label = "Remove Message",
                    Style = ButtonStyle.Danger,
                    DisableOnClick = true,
                    Action = async messageComponent =>
                    {
                        MessageComponent menu = selectMenuService.CreateSelectMenu("Select", options,
                            async (item, messageComponent) =>
                            {
                                await serverSetupService.RemoveMessage(guild.Id, Guid.Parse(item));
                            }, "Message has been removed.");

                        await messageComponent.RespondAsync("Select what message to remove:", components: menu);
                    }
                }

            ];
            MessageComponent buttonMessageComponent = buttonsService.CreateButtonRow(buttons);
            
            await RespondAsync(embed: embedBuilder.Build(), components: buttonMessageComponent);
        }

        [SlashCommand("toggle", "Enables/disables welcome/goodbye messages")]
        public async Task SetupWgToggle()
        {
            Server server = dbContext.GetOrCreateServer(Context.Guild);

            if (!server.WelcomeMessageEnabled)
            {
                if (server.WelcomeGoodbyeChannelId == null)
                {
                    await RespondAsync("No welcome/goodbye channel is set!");
                    return;
                }

                bool welcomeMessage =
                    dbContext.ServerMessages.Any(x => x.ServerId == server.Id && x.Type == MessageType.Welcome);
                bool goodbyeMessage = dbContext.ServerMessages.Any(x => x.ServerId == server.Id && x.Type == MessageType.Goodbye);

                if (!welcomeMessage || !goodbyeMessage)
                {
                    await RespondAsync("No welcome/goodbye messages are set!");
                    return;
                }
            }
            
            server.WelcomeMessageEnabled = !server.WelcomeMessageEnabled;
            await dbContext.SaveChangesAsync();
            
            if(server.WelcomeMessageEnabled)
                await RespondAsync($"Welcome/goodbye messages are now enabled.");
            else
                await RespondAsync("Welcome/goodbye messages are now disabled.");
        }

        private class SetupWgAddMessage
        {
            [ModalProperty("Message", "Enter Message", true, ModalPropertyType.Text)]
            public string Message;
        }
    }

    [Group("autovc", "Commands related to AutoVCs")]
    public class AutoVcSubGroupModule : InteractionModuleBase<SocketInteractionContext>
    {
        private const string ObjectName = "AutoVC";
        
        private readonly ILogger<AutoVcSubGroupModule> logger;
        private readonly AutoVcService autoVcService;
        private readonly ServerService serverService;
        
        public AutoVcSubGroupModule(ILogger<AutoVcSubGroupModule> logger, AutoVcService autoVcService, ServerService serverService)
        {
            this.logger = logger;
            this.autoVcService = autoVcService;
            this.serverService = serverService;
        }
        
        [SlashCommand("create", "Creates a new AutoVc channel")]
        public async Task CreateAutoVc(string baseName, int maxChannels = 3, int maxUsers = 25)
        {
            SocketGuild guild = Context.Guild;
            Server server = serverService.GetOrCreateServer(guild.Id);

            try
            {
                //Create new channel
                RestVoiceChannel vcChannel = await guild.CreateVoiceChannelAsync($"➕ New {baseName} VC");
                autoVcService.CreateAutoVc(server.Id, vcChannel.Id, baseName, maxChannels, maxUsers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed creating new AutoVC on server {ServerId}!", server.Id);
                await RespondAsync(Messages.CreatedFailed(ObjectName));
                return;
            }
            
            await RespondAsync(Messages.CreateSuccessful(ObjectName));
        }
    }
}