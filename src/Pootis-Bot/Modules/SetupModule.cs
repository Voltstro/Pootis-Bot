using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Helper;
using Pootis_Bot.Services;
using Pootis_Bot.Services.Interactions.Buttons;
using Pootis_Bot.Services.Interactions.Modal;
using Pootis_Bot.Services.Interactions.SelectMenu;
using Pootis_Bot.Shared.Helper;
using Pootis_Bot.Shared.Models;
using Emoji = Pootis_Bot.Core.Discord.Emoji;
using MessageType = Pootis_Bot.Shared.Messages.MessageType;

namespace Pootis_Bot.Modules;

[CommandContextType(InteractionContextType.Guild)]
[Group("setup", "Commands for server setup")]
public class SetupModule : InteractionModuleBase<SocketInteractionContext>
{
    [Group("rule-reaction", "Commands related to rule reactions")]
    public class RuleReactionSubCommandGroupModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger<RuleReactionSubCommandGroupModule> logger;
        private readonly ServerService serverService;
    
        public RuleReactionSubCommandGroupModule(ILogger<RuleReactionSubCommandGroupModule> logger, ServerService serverService)
        {
            this.logger = logger;
            this.serverService = serverService;
        }

        [SlashCommand("status", "Gets the status of rule reaction setup")]
        public async Task SetupRrStatus()
        {
            SocketGuild guild = Context.Guild;
            Server server = serverService.GetOrCreateServer(guild.Id);
            
            IMessage? message = null;
            SocketTextChannel? channel = server.RuleReactionChannelId.HasValue ? guild.GetTextChannel(server.RuleReactionChannelId.Value) : null;
            if (channel != null && server.RuleReactionMessageId.HasValue)
                message = await channel.GetMessageAsync(server.RuleReactionMessageId.Value);
        
            //Role
            SocketRole? role = null;
            if(server.RuleReactionRoleId.HasValue)
                role = guild.GetRole(server.RuleReactionRoleId.Value);

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithTitle("Rule Reaction Status");
            embedBuilder.WithDescription($"Status of Rule Reaction for **{guild.Name}**");
            embedBuilder.AddField("Enabled?", server.RuleReactionEnabled);
            embedBuilder.AddField("Message", message == null ? "No Message Set" : $"[Link]({message.GetMessageUrl()})");
            embedBuilder.AddField("Emoji", string.IsNullOrEmpty(server.RuleReactionEmoji) ? "No Emoji Set" : server.RuleReactionEmoji);
            embedBuilder.AddField("Role", role == null ? "No Role Set" : role.Name);
        
            await RespondAsync(embed: embedBuilder.Build());
        }
        
        [RequireBotPermission(GuildPermission.ViewChannel)]
        [RequireUserPermission(GuildPermission.ViewChannel)]
        [SlashCommand("message", "Sets the message that rule reactions should occur on")]
        public async Task SetupRrMessage(SocketTextChannel channel, [Summary("messageId", "Discord ID of the message to look for a reaction on.")] string messageId)
        {
            // Validate messageId
            bool valid = ulong.TryParse(messageId, out ulong validMessageId);
            if (!valid)
            {
                await RespondAsync(Messages.ValidationFailed("messageId", "a valid number"));
                return;
            }
            
            //Attempt to get message
            IMessage? message = await channel.GetMessageAsync(validMessageId);
            if (message == null)
            {
                await RespondAsync(Messages.ValidationFailed("messageId", "a valid ID of message"));
                return;
            }

            try
            {
                //Set channel and message
                Server server = serverService.GetOrCreateServer(Context.Guild.Id);
                server.RuleReactionChannelId = channel.Id;
                server.RuleReactionMessageId = validMessageId;
                serverService.UpdateServer(server);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to set rule reaction message");
                await RespondAsync(Messages.SetFailed("rule reaction message"));
                return;
            }
            
            await RespondAsync(Messages.SetSuccessful("rule reaction message",
                $"channel {channel.Mention}({messageId})"));
        }

        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [SlashCommand("role", "Sets what role is given on reaction")]
        public async Task SetupRrRole(SocketRole role)
        {
            SocketGuild guild = Context.Guild;

            try
            {
                Server server = serverService.GetOrCreateServer(guild.Id);
                server.RuleReactionRoleId = role.Id;
                serverService.UpdateServer(server);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to set rule reaction role");
                await RespondAsync(Messages.SetFailed("rule reaction role"));
                return;
            }
            
            await RespondAsync(Messages.SetSuccessful("rule reaction role", $"**{role.Name}**"));
        }

        [SlashCommand("emoji", "Sets what emoji is required to be reacted with")]
        public async Task SetupRrEmoji(Emoji emoji)
        {
            try
            {
                Server server = serverService.GetOrCreateServer(Context.Guild.Id);
                server.RuleReactionEmoji = emoji.ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to set rule reaction emoji");
                await RespondAsync(Messages.SetFailed("rule reaction emoji"));
                return;
            }
            
            await RespondAsync(Messages.SetSuccessful("rule reaction emoji", $"**{emoji}**"));
        }

        [SlashCommand("toggle", "Enables/disables rule reaction")]
        public async Task SetupRrEnable()
        {
            Server server = serverService.GetOrCreateServer(Context.Guild.Id);

            //Need to check that everything is all good (if we are enabling)
            if (!server.RuleReactionEnabled)
            {
                if (server.RuleReactionChannelId == null || server.RuleReactionMessageId == null)
                {
                    await RespondAsync(Messages.FeatureCannotBeEnable("rule reaction", "channel and message to be set first"));
                    return;
                }

                if (server.RuleReactionRoleId == null)
                {
                    await RespondAsync(Messages.FeatureCannotBeEnable("rule reaction", "role to be set first"));
                    return;
                }

                if (server.RuleReactionEmoji == null)
                {
                    await RespondAsync(Messages.FeatureCannotBeEnable("rule reaction", "emoji to be set first"));
                    return;
                }
            }
            
            //Toggle
            server.RuleReactionEnabled = !server.RuleReactionEnabled;
            serverService.UpdateServer(server);
            
            await RespondAsync(server.RuleReactionEnabled ? Messages.FeatureEnabled("rule reaction") : Messages.FeatureDisabled("rule reaction"));
        }
    }

    [Group("welcome-goodbye", "Commands related to welcome and goodbye messages")]
    public class WelcomeGoodbyeSubGroupModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger<WelcomeGoodbyeSubGroupModule> logger;
        private readonly ServerService serverService;
        private readonly ServerMessageService serverMessageService;
        private readonly ButtonsService buttonsService;
        private readonly SelectMenuService selectMenuService;
        private readonly ModalService modalService;
    
        public WelcomeGoodbyeSubGroupModule(
            ILogger<WelcomeGoodbyeSubGroupModule> logger,
            ServerService serverService,
            ServerMessageService serverMessageService,
            ButtonsService buttonsService,
            SelectMenuService selectMenuService,
            ModalService modalService)
        {
            this.logger = logger;
            this.serverService = serverService;
            this.serverMessageService =  serverMessageService;
            this.buttonsService = buttonsService;
            this.selectMenuService = selectMenuService;
            this.modalService = modalService;
        }

        [SlashCommand("channel", "Sets the channel used for welcome and goodbye messages")]
        public async Task SetupWgChannel(SocketTextChannel channel)
        {
            SocketGuild guild = Context.Guild;
            try
            {
                Server server = serverService.GetOrCreateServer(guild.Id);

                server.WelcomeGoodbyeChannelId = channel.Id;
                serverService.UpdateServer(server);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to set welcome goodbye channel");
                await RespondAsync(Messages.SetFailed("welcome goodbye channel"));
                return;
            }
            
            await Context.Interaction.RespondAsync(Messages.SetSuccessful("welcome goodbye channel", channel.Mention));
        }

        [SlashCommand("messages", "List all current message of type specified.")]
        public async Task SetupWgMessages(MessageType messageType)
        {
            SocketGuild guild = Context.Guild;
            Server server = serverService.GetOrCreateServer(guild.Id);
            ServerMessage[] messages = await serverMessageService.GetAllMessagesOfType(server, messageType);
            
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
                            await serverMessageService.AddMessage(server, messageType, addMessage.Message);
                            await socketModal.RespondAsync(Messages.CreateSuccessful(messageType.ToString().ToLower()));
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
                                await serverMessageService.DeleteMessage(server, Guid.Parse(item));
                            }, Messages.DeleteSuccessful(messageType.ToString().ToLower()));

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
            Server server = serverService.GetOrCreateServer(Context.Guild.Id);

            if (!server.WelcomeMessageEnabled)
            {
                if (server.WelcomeGoodbyeChannelId == null)
                {
                    await RespondAsync(Messages.FeatureCannotBeEnable("welcome/goodbye messages", "a channel needs to be set"));
                    return;
                }

                bool welcomeMessage = await serverMessageService.GetIsAnyMessageOfType(server, MessageType.Welcome);
                bool goodbyeMessage = await serverMessageService.GetIsAnyMessageOfType(server, MessageType.Goodbye);

                if (!welcomeMessage || !goodbyeMessage)
                {
                    await RespondAsync(Messages.FeatureCannotBeEnable("welcome/goodbye messages", "both a welcome and a goodbye message to be created"));
                    return;
                }
            }
            
            server.WelcomeMessageEnabled = !server.WelcomeMessageEnabled;
            serverService.UpdateServer(server);
            
            await RespondAsync(server.WelcomeMessageEnabled ? Messages.FeatureEnabled("welcome/goodbye messages") : Messages.FeatureDisabled("welcome/goodbye messages"));
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
        
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
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