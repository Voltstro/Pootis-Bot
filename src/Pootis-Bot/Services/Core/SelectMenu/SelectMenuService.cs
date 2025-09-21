using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Services.Core.Client;

namespace Pootis_Bot.Services.Core.SelectMenu;

/// <summary>
///     Service for handling select menus
/// </summary>
public sealed class SelectMenuService
{
    private const string ItemIdPrefix = "PootisBotSm";
    private const string ItemOptionIdPrefix = "PootisBotSmi";

    private readonly ILogger<SelectMenuService> logger;
    private readonly Dictionary<string, SelectMenuItem> selectActions;
    
    public SelectMenuService( ILogger<SelectMenuService> logger, ClientService clientService)
    {
        this.logger = logger;
        selectActions = new Dictionary<string, SelectMenuItem>();
        
        clientService.DiscordClient.SelectMenuExecuted += OnSelectMenuExecuted;
    }

    /// <summary>
    ///     Creates a select menu
    /// </summary>
    /// <param name="placeholder"></param>
    /// <param name="options"></param>
    /// <param name="onSelect"></param>
    /// <returns></returns>
    public MessageComponent CreateSelectMenu(string placeholder, Dictionary<string, string> options, Func<string, SocketMessageComponent, Task> onSelect, string? responseMessage = null)
    {
        string menuItemId = $"{ItemIdPrefix}.{Guid.NewGuid()}";
        
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
            string optionId = $"{ItemOptionIdPrefix}.{Guid.NewGuid()}";
            menu.AddOption(option.Key, optionId);
            optionIdToValueMapping.Add(optionId, option.Value);
        }
        
        ComponentBuilder builder = new ComponentBuilder()
            .WithSelectMenu(menu);
        
        selectActions.Add(menuItemId, new SelectMenuItem(onSelect, menu, optionIdToValueMapping, responseMessage));
        
        return builder.Build();
    }

    private async Task OnSelectMenuExecuted(SocketMessageComponent messageComponent)
    {
        try
        {
            string customId = messageComponent.Data.CustomId;
            if (!customId.StartsWith(ItemIdPrefix))
                return;

            KeyValuePair<string, SelectMenuItem>? item = selectActions.FirstOrDefault(x => x.Key == customId);
            if (item == null)
                return;

            SelectMenuItem selectMenuItem = item.Value.Value;
            
            //Get selected value
            string selectedId = messageComponent.Data.Values.First();

            //Update select menu to be disabled
            SelectMenuBuilder selectMenuBuilder = selectMenuItem.SelectMenu;
            selectMenuBuilder.WithDisabled(true);
            
            if (selectMenuItem.ResponseMessage != null)
                selectMenuBuilder.Placeholder = selectMenuItem.ResponseMessage;

            ComponentBuilder builder = new ComponentBuilder()
                .WithSelectMenu(selectMenuBuilder);

            await messageComponent.UpdateAsync(x => x.Components = builder.Build());

            //Get actual value
            KeyValuePair<string, string> selectedMenuItemValue = selectMenuItem.OptionKeysToValue.First(x => x.Key == selectedId);

            //Invoke action
            await selectMenuItem.Action.Invoke(selectedMenuItemValue.Value, messageComponent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while handling select menu executed event!");
        }
    }
}