using System.Collections.Generic;
using Discord;

namespace Pootis_Bot.Services.Interactions.Buttons;

public struct ButtonGroup
{
    public ButtonGroup(List<ButtonItem> buttons, ComponentBuilder componentBuilder)
    {
        Buttons = buttons;
        ComponentBuilder = componentBuilder;
    }
    
    public List<ButtonItem> Buttons { get; set; }
    public ComponentBuilder ComponentBuilder { get; set; }
}