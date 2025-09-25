namespace Pootis_Bot.Services.Interactions;

public abstract class InteractionDataBase
{
    public InteractionDataBase(string itemId)
    {
        ItemId = itemId;
    }
    
    public string ItemId { get; }
    public bool Handled { get; set; }
}