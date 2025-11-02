using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Pootis_Bot.Services.Interactions.Buttons;

public struct Button
{
    public string Label { get; init; }
    
    public ButtonStyle Style { get; init; }
    
    public Func<SocketMessageComponent, Task> Action { get; init; }
    
    public bool DisableOnClick { get; init; }
}