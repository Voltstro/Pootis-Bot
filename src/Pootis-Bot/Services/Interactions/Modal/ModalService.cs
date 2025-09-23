using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Services.Core.Client;

namespace Pootis_Bot.Services.Interactions.Modal;

public class ModalService
{
    private const string ModalIdPrefix = "PootisBotModal";
    
    private readonly ILogger<ModalService> logger;

    private List<ModalItem> modals;
    
    public ModalService(ILogger<ModalService> logger, ClientService clientService)
    {
        this.logger = logger;
        modals = new List<ModalItem>();
        
        clientService.DiscordClient.ModalSubmitted += OnModalSubmitted;
    }
    
    public Discord.Modal CreateModal<T>(string title, Func<T, SocketModal, Task> onSubmit)
        where T : class
    {
        string modalId = $"{ModalIdPrefix}.{Guid.NewGuid()}";
        
        ModalBuilder builder = new();
        builder.WithTitle(title);
        builder.WithCustomId(modalId);

        Type type = typeof(T);
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach (FieldInfo field in fields)
        {
            ModalPropertyAttribute? modalPropertyAttribute = field.GetCustomAttribute<ModalPropertyAttribute>();
            if (modalPropertyAttribute == null || field.FieldType != typeof(string))
            {
                logger.LogWarning("Field {FieldName} on class {ClassName} either does not have the ModalPropertyAttribute attribute, or is not a type of string.", field.Name, type.Name);
                continue;
            }

            TextInputBuilder textInputBuilder = new();
            textInputBuilder.WithCustomId(field.Name);
            textInputBuilder.WithLabel(modalPropertyAttribute.Title);
            textInputBuilder.WithPlaceholder(modalPropertyAttribute.Placeholder);
            textInputBuilder.WithRequired(modalPropertyAttribute.Required);
            builder.AddTextInput(textInputBuilder);
        }
        
        //Can't just cast directly as Func<object, Task>, so create a wrapper func
        //Not great, but works
        Func<object, SocketModal, Task> onSubmitCast = async (objValue, socketModal) => await onSubmit(objValue as T, socketModal);
        ModalItem modalItem = new(modalId, type, onSubmitCast);
        modals.Add(modalItem);

        return builder.Build();
    }
    
    private async Task OnModalSubmitted(SocketModal socketModal)
    {
        SocketModalData modalData = socketModal.Data;
        if (!modalData.CustomId.StartsWith(ModalIdPrefix))
            return;

        try
        {
            ModalItem? modalItem = modals.FirstOrDefault(x => x.Id == modalData.CustomId);
            if (modalItem == null)
            {
                logger.LogWarning("Modal item with id {Id} does not exist", modalData.CustomId);
                return;
            }
            
            //Create type
            Type modalType = modalItem.DataClassType;
            object dataClassInstance = Activator.CreateInstance(modalType)!;

            foreach (SocketMessageComponentData socketMessageComponentData in modalData.Components)
            {
                FieldInfo? field = modalType.GetField(socketMessageComponentData.CustomId);
                if (field == null)
                {
                    logger.LogWarning("Field {PropertyName} requested from modal {ModalId} on type {TypeName} does not exist!", socketMessageComponentData.CustomId, modalItem.Id, modalType.Name);
                    continue;
                }

                field.SetValue(dataClassInstance, socketMessageComponentData.Value);
            }

            await modalItem.OnSubmit.Invoke(dataClassInstance, socketModal);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling modal!");
        }
    }

    private record ModalItem(string Id, Type DataClassType, Func<object, SocketModal, Task> OnSubmit);
}