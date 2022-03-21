using System.Reflection;
using Spectre.Console;

namespace Pootis_Bot.Console.ConfigMenus.TypeReaders;

internal class BoolTypeReader : ITypeReader
{
    public string ValidationErrorMessage => "[red]That is not a valid input[/]";
    
    public ValidationResult Validate(string input)
    {
        input = input.ToLower();
        if (!bool.TryParse(input, out _))
        {
            if(input is "true" or "false" or "yes" or "no")
                return ValidationResult.Success();
        }
        
        return ValidationResult.Error("[red]Input needs to be 'true' or 'yes' for yes or 'false' or 'no' for no[/]");
    }

    public void SetProperty(PropertyInfo type, object editingObject, string input)
    {
        input = input.ToLower();
        if (!bool.TryParse(input, out bool value))
        {
            if (input is "true" or "yes")
                value = true;
            if (input is "false" or "no")
                value = false;
        }
        
        type.SetValue(editingObject, value);
    }
}