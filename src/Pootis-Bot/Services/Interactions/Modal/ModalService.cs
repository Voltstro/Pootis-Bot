using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Services.Core.Client;

namespace Pootis_Bot.Services.Interactions.Modal;

/// <summary>
///     Interaction service for handling Modal's from Discord
/// </summary>
public sealed class ModalService : InteractionServiceBase<ModalService, ModalInteractionData>
{
    protected override string InteractionServiceName => "Modal";
    
    public ModalService(
        ILogger<ModalService> logger,
        IMemoryCache memoryCache,
        ClientService clientService)
        : base(logger, memoryCache)
    {
        clientService.DiscordClient.ModalSubmitted += OnModalSubmitted;
    }
    
    /// <summary>
    ///     Creates a Discord modal
    /// </summary>
    /// <param name="title">The title of the modal</param>
    /// <param name="onSubmit">Func to invoke when the modal is submitted</param>
    /// <typeparam name="T">Class representing the modal form</typeparam>
    /// <returns></returns>
    public Discord.Modal CreateModal<T>(string title, Func<T, SocketModal, Task> onSubmit)
        where T : class
    {
        string modalId = GenerateItemId();
        
        ModalBuilder builder = new();
        builder.WithTitle(title);
        builder.WithCustomId(modalId);

        Type type = typeof(T);
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach (FieldInfo field in fields)
        {
            ModalPropertyAttribute? modalPropertyAttribute = field.GetCustomAttribute<ModalPropertyAttribute>();
            if (modalPropertyAttribute == null)
            {
                Logger.LogWarning("Field {FieldName} on class {ClassName} does not have the ModalPropertyAttribute attribute.", field.Name, type.Name);
                continue;
            }

            Type fieldType = field.FieldType;
            if (fieldType == typeof(string))
            {
                TextInputBuilder textInputBuilder = new();
                textInputBuilder.WithCustomId(field.Name);
                textInputBuilder.WithLabel(modalPropertyAttribute.Title);
                textInputBuilder.WithPlaceholder(modalPropertyAttribute.Placeholder);
                textInputBuilder.WithRequired(modalPropertyAttribute.Required);
                builder.AddTextInput(textInputBuilder);
            }
            else
            {
                throw new ArgumentException($"Field {fieldType.Name} on class {type.Name} is not supported!");
            }
        }
        
        //Can't just cast directly as Func<object, Task>, so create a wrapper func
        //Not great, but works
        Func<object, SocketModal, Task> onSubmitCast = async (objValue, socketModal) => await onSubmit(objValue as T, socketModal);
        
        ModalInteractionData interactionData = new(modalId, type, onSubmitCast);
        StoreInteraction(interactionData);

        return builder.Build();
    }
    
    private async Task OnModalSubmitted(SocketModal socketModal)
    {
        ModalInteractionData? modalItem = TryRetrieveInteraction(socketModal.Data.CustomId);
        if (modalItem == null)
            return;
        
        try
        {
            Type modalType = modalItem.DataClassType;
            object dataClassInstance = Activator.CreateInstance(modalType)!;
            
            foreach (SocketMessageComponentData socketMessageComponentData in socketModal.Data.Components)
            {
                FieldInfo? field = modalType.GetField(socketMessageComponentData.CustomId);
                if (field == null)
                {
                    Logger.LogWarning("Field {PropertyName} requested from modal {ModalId} on type {TypeName} does not exist!", socketMessageComponentData.CustomId, modalItem.ItemId, modalType.Name);
                    continue;
                }
                
                Type fieldType = field.FieldType;
                if (fieldType == typeof(string))
                {
                    field.SetValue(dataClassInstance, socketMessageComponentData.Value);
                }
                else
                {
                    throw new ArgumentException($"Field {fieldType.Name} on {modalItem.ItemId} is not supported!");
                }
            }

            await modalItem.OnSubmit.Invoke(dataClassInstance, socketModal);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling modal interaction!");
        }
    }
}