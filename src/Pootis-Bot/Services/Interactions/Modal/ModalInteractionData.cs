using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Pootis_Bot.Services.Interactions.Modal;

public class ModalInteractionData : InteractionDataBase
{
    public ModalInteractionData(string itemId, Type dataClassType, Func<object, SocketModal, Task> onSumbit)
        : base(itemId)
    {
        DataClassType = dataClassType;
        OnSubmit = onSumbit;
    }
    
    public Type DataClassType { get; set; }
    public Func<object, SocketModal, Task> OnSubmit { get; set; }
}