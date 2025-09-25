using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Pootis_Bot.Services.Interactions;

/// <summary>
///     Base implmentation for interaction services
/// </summary>
/// <typeparam name="TInteractionService"></typeparam>
/// <typeparam name="TStoreItem"></typeparam>
public abstract class InteractionServiceBase<TInteractionService, TStoreItem>
    where TInteractionService : class
    where TStoreItem : InteractionDataBase
{
    private const string ItemPrefix = "PootisBot";
    
    protected readonly ILogger<TInteractionService> Logger;
    
    private readonly IMemoryCache memoryCache;

    protected abstract string InteractionServiceName { get; }
    
    public InteractionServiceBase(ILogger<TInteractionService> logger, IMemoryCache memoryCache)
    {
        Logger = logger;
        this.memoryCache = memoryCache;
    }

    protected void StoreInteraction(TStoreItem storeItem)
    {
        string itemId = storeItem.ItemId;
        //storeItem.ItemId = itemId;
        storeItem.Handled = false;

        //TODO: Item expiry
        Logger.LogDebug("Stored item {ItemId} for service {ServiceName}", storeItem.ItemId, InteractionServiceName);
        memoryCache.Set(itemId, storeItem);
    }

    protected TStoreItem? TryRetrieveInteraction(string customId)
    {
        if (!customId.StartsWith($"{ItemPrefix}{InteractionServiceName}") || !memoryCache.TryGetValue(customId, out TStoreItem item))
        {
            Logger.LogWarning("Requested interaction {CustomId} does not exist.", customId);
            return null;
        }

        //TODO: Make item expiry longer
        item.Handled = true;
        return item;
    }

    protected string GenerateItemId(string? item = null)
    {
        string itemId = $"{ItemPrefix}{InteractionServiceName}";
        if (!string.IsNullOrWhiteSpace(item))
            itemId += $".{item}";
        
        itemId += $".{Guid.NewGuid()}";
        return itemId;
    }
}