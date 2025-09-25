using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Services.Core.Client;

namespace Pootis_Bot.Services.Interactions.SelectMenu;

/// <summary>
///     Interaction service for handling select menus
/// </summary>
public sealed class SelectMenuService : InteractionServiceBase<SelectMenuService, SelectMenuInteractionData>
{
    protected override string InteractionServiceName => "SM";
    
    public SelectMenuService(ILogger<SelectMenuService> logger, IMemoryCache memoryCache, ClientService clientService)
        : base(logger, memoryCache)
    {
        clientService.DiscordClient.SelectMenuExecuted += OnSelectMenuExecuted;
    }

    /// <summary>
    ///     Creates a select menu
    /// </summary>
    /// <param name="placeholder"></param>
    /// <param name="options"></param>
    /// <param name="onSelect"></param>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    public MessageComponent CreateSelectMenu(string placeholder, Dictionary<string, string> options, Func<string, SocketMessageComponent, Task> onSelect, string? responseMessage = null)
    {
        string menuItemId = GenerateItemId();
        SelectMenuBuilder menu = new()
        {
            CustomId = menuItemId,
            Placeholder = placeholder,
            MaxValues = 1,
            MinValues = 1,
        };

        Dictionary<string, string> optionIdToValueMapping = new();
        foreach (KeyValuePair<string, string> option in options)
        {
            string optionId = GenerateItemId("Option");
            menu.AddOption(option.Key, optionId);
            optionIdToValueMapping.Add(optionId, option.Value);
        }
        
        ComponentBuilder builder = new ComponentBuilder()
            .WithSelectMenu(menu);

        SelectMenuInteractionData interactionData = new(menuItemId, menu, optionIdToValueMapping, onSelect, responseMessage);
        StoreInteraction(interactionData);

        return builder.Build();
    }

    private async Task OnSelectMenuExecuted(SocketMessageComponent messageComponent)
    {
        SelectMenuInteractionData? selectMenuInteraction = TryRetrieveInteraction(messageComponent.Data.CustomId);
        if(selectMenuInteraction == null)
            return;

        try
        {
            //Get selected value
            string selectedId = messageComponent.Data.Values.First();

            //Update select menu to be disabled
            SelectMenuBuilder selectMenuBuilder = selectMenuInteraction.SelectMenuBuilder;
            selectMenuBuilder.WithDisabled(true);

            if (selectMenuInteraction.ResponseMessage != null)
                selectMenuBuilder.Placeholder = selectMenuInteraction.ResponseMessage;

            ComponentBuilder builder = new ComponentBuilder()
                .WithSelectMenu(selectMenuBuilder);

            await messageComponent.UpdateAsync(x => x.Components = builder.Build());

            //Invoke with selected value
            string selectedValue = selectMenuInteraction.OptionIdToValueMapping.FirstOrDefault(x => x.Key == selectedId)
                .Value;
            await selectMenuInteraction.OnAction.Invoke(selectedValue, messageComponent);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling selected menu!");
        }
    }
}