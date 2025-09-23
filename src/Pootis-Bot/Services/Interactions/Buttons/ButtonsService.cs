using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Services.Core.Client;

namespace Pootis_Bot.Services.Interactions.Buttons;

/// <summary>
///     Service for handling select menus
/// </summary>
public sealed class ButtonsService
{
    private const string ButtonIdPrefix = "PootisBotBtn";

    private readonly ILogger<ButtonsService> logger;
    private readonly List<ButtonGroup> buttonActions;
    
    public ButtonsService(ILogger<ButtonsService> logger, ClientService clientService)
    {
        this.logger = logger;
        buttonActions = new List<ButtonGroup>();
        
        clientService.DiscordClient.ButtonExecuted += OnButtonExecuted;
    }

    /// <summary>
    ///     Creates a row of buttons
    /// </summary>
    /// <param name="buttons"></param>
    /// <returns></returns>
    public MessageComponent CreateButtonRow(List<Button> buttons)
    {
        ComponentBuilder builder = new();

        List<ButtonItem> buttonList = new List<ButtonItem>();
        foreach (Button button in buttons)
        {
            string optionId = $"{ButtonIdPrefix}.{Guid.NewGuid()}";
            
            ButtonBuilder builderButton = new ButtonBuilder();
            builderButton.CustomId = optionId;
            builderButton.Label = button.Label;
            builderButton.Style = button.Style;
            
            builder.WithButton(builderButton);
            
            buttonList.Add(new ButtonItem(optionId, builderButton, button.Action, button.DisableOnClick));
        }
        
        buttonActions.Add(new ButtonGroup(buttonList, builder));
        
        return builder.Build();
    }
    
    private async Task OnButtonExecuted(SocketMessageComponent messageComponent)
    {
        try
        {
            string customId = messageComponent.Data.CustomId;
            if (!customId.StartsWith(ButtonIdPrefix))
                return;
            
            ButtonGroup? buttonGroup = buttonActions.FirstOrDefault(x => x.Buttons.Any(x => x.CustomId == customId));
            if (buttonGroup == null)
                return;

            ButtonItem button = buttonGroup.Value.Buttons.First(x => x.CustomId == customId);

            //Disable button is required
            if (button.DisableOnClick)
            {
                ComponentBuilder componentBuilder = buttonGroup.Value.ComponentBuilder;
                componentBuilder.RemoveComponent(customId);

                ButtonBuilder buttonBuilder = button.ButtonBuilder;
                buttonBuilder.IsDisabled = true;
                componentBuilder.WithButton(buttonBuilder);
                
                //await messageComponent.UpdateAsync(x => x.Components = componentBuilder.Build());
                //await messageComponent.UpdateAsync(x => x.)
            }
            
            //Do action
            await button.Action(messageComponent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while executing button");
        }
    }
}