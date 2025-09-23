using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Pootis_Bot.Services.Interactions.Buttons;

public struct ButtonItem
{
    public ButtonItem(
        string customId,
        ButtonBuilder builder,
        Func<SocketMessageComponent, Task> action,
        bool disableOnClick)
    {
        CustomId = customId;
        ButtonBuilder = builder;
        Action = action;
        DisableOnClick = disableOnClick;
    }
    
    public string CustomId { get; }
    
    public ButtonBuilder ButtonBuilder { get; }
    
    public Func<SocketMessageComponent, Task> Action { get; }
    
    public bool DisableOnClick { get; } 
}