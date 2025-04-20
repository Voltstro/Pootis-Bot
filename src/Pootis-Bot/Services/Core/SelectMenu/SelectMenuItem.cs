using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Pootis_Bot.Services.Core.SelectMenu;

public class SelectMenuItem
{
    public SelectMenuItem(Func<string, Task> action, SelectMenuBuilder selectMenu, Dictionary<string, string> optionKeysToValue)
    {
        Action = action;
        SelectMenu = selectMenu;
        OptionKeysToValue = optionKeysToValue;
    }
    
    public Dictionary<string, string> OptionKeysToValue { get; }
    
    public Func<string, Task> Action { get; }
    
    public SelectMenuBuilder SelectMenu { get; }
}