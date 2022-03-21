using System.Reflection;
using Spectre.Console;

namespace Pootis_Bot.Console.ConfigMenus.TypeReaders;

internal class StringTypeReader : ITypeReader
{
    public string ValidationErrorMessage => "[red]That is not a valid input[/]";

    public ValidationResult Validate(string input)
    {
        if(string.IsNullOrWhiteSpace(input))
            return ValidationResult.Error("[red]Input cannot just be empty or white space!");
        
        return ValidationResult.Success();
    }

    public void SetProperty(PropertyInfo type, object editingObject, string input)
    {
        type.SetValue(editingObject, input);
    }
}