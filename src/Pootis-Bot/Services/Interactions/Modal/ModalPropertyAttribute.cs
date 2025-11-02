using System;

namespace Pootis_Bot.Services.Interactions.Modal;

[AttributeUsage(AttributeTargets.Field)]
public class ModalPropertyAttribute : Attribute
{
    public ModalPropertyAttribute(string title, string placeholder, bool required, ModalPropertyType propertyType)
    {
        Title = title;
        Placeholder = placeholder;
        Required = required;
        PropertyType = propertyType;
    }
    
    public string Title { get; }
    public string Placeholder { get; }
    public bool Required { get; }
    public ModalPropertyType PropertyType { get; }
}