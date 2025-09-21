using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Pootis_Bot.Services.Core.SelectMenu;

public class SelectMenuItem
{
    public SelectMenuItem(Func<string, SocketMessageComponent, Task> action, SelectMenuBuilder selectMenu, Dictionary<string, string> optionKeysToValue, string? responseMessage)
    {
        Action = action;
        SelectMenu = selectMenu;
        OptionKeysToValue = optionKeysToValue;
        ResponseMessage = responseMessage;
    }
    
    public Dictionary<string, string> OptionKeysToValue { get; }
    
    public Func<string, SocketMessageComponent, Task> Action { get; }
    
    public SelectMenuBuilder SelectMenu { get; }
    
    public string? ResponseMessage { get; }
}