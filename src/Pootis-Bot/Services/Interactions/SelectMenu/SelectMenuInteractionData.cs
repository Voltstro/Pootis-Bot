using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Pootis_Bot.Services.Interactions.SelectMenu;

public class SelectMenuInteractionData : InteractionDataBase
{
    public SelectMenuInteractionData(
        string itemId,
        SelectMenuBuilder selectMenuBuilder,
        Dictionary<string, string> optionIdToValueMapping,
        Func<string, SocketMessageComponent, Task> onAction,
        string? responseMessage) : base(itemId)
    {
        SelectMenuBuilder = selectMenuBuilder;
        OptionIdToValueMapping = optionIdToValueMapping;
        OnAction = onAction;
        ResponseMessage = responseMessage;
    }
    
    public SelectMenuBuilder SelectMenuBuilder { get; }
    
    public Dictionary<string, string> OptionIdToValueMapping { get; }
    
    public Func<string, SocketMessageComponent, Task> OnAction { get; }
    
    public string? ResponseMessage { get; }
}