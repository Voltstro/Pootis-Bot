using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Services.Core.Client;

namespace Pootis_Bot.Services.Interactions.Buttons;

/// <summary>
///     Interaction service for handling buttons
/// </summary>
public sealed class ButtonsService : InteractionServiceBase<ButtonsService, ButtonInteractionData>
{
    protected override string InteractionServiceName => "Button";
    
    public ButtonsService(
        ILogger<ButtonsService> logger, 
        IMemoryCache memoryCache,
        ClientService clientService)
        : base(logger, memoryCache)
    {
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
        
        foreach (Button button in buttons)
        {
            string buttonId = GenerateItemId();
            ButtonBuilder buttonBuilder = new()
            {
                CustomId = buttonId,
                Label = button.Label,
                Style = button.Style
            };

            builder.WithButton(buttonBuilder);
            StoreInteraction(new ButtonInteractionData(buttonId, button.Action));
        }
        
        return builder.Build();
    }
    
    private async Task OnButtonExecuted(SocketMessageComponent messageComponent)
    {
        ButtonInteractionData? buttonsData = TryRetrieveInteraction(messageComponent.Data.CustomId);
        if (buttonsData == null)
            return;

        try
        {
            await buttonsData.Action.Invoke(messageComponent);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling button action!");
        }
    }
}