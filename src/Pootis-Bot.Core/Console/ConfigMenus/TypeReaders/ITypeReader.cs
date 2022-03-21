using System.Reflection;
using Spectre.Console;

namespace Pootis_Bot.Console.ConfigMenus.TypeReaders;

internal interface ITypeReader
{
    public string ValidationErrorMessage { get; }
    
    public ValidationResult Validate(string input);

    public void SetProperty(PropertyInfo type, object editingObject, string input);
}