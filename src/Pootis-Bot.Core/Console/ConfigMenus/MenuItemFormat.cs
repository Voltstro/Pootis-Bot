using System;
using System.Diagnostics.CodeAnalysis;

namespace Pootis_Bot.Console.ConfigMenus;

/// <summary>
///     Tells the <see cref="ConsoleConfigMenu{T}" /> how to format this property in the selection menu
///     <para>If you don't use this the <see cref="ConsoleConfigMenu{T}" /> will just use the name of the property</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class MenuItemFormat : Attribute
{
    internal readonly string FormattedName;

    /// <summary>
    ///     Creates a new <see cref="MenuItemFormat" />
    /// </summary>
    /// <param name="formatted">The formatted <see cref="string" /> that will be shown in the menu.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public MenuItemFormat([DisallowNull] string formatted)
    {
        if (string.IsNullOrWhiteSpace(formatted))
            throw new ArgumentNullException(nameof(formatted));

        FormattedName = formatted;
    }
}