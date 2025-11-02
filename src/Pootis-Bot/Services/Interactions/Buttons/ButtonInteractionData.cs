using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Pootis_Bot.Services.Interactions.Buttons;

public class ButtonInteractionData : InteractionDataBase
{
    public ButtonInteractionData(string itemId, Func<SocketMessageComponent, Task> action) : base(itemId)
    {
        Action = action;
    }
    
    public Func<SocketMessageComponent, Task> Action { get; }
}