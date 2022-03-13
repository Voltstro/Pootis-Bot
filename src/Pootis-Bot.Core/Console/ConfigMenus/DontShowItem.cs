using System;

namespace Pootis_Bot.Console.ConfigMenus;

/// <summary>
///     Makes the <see cref="ConsoleConfigMenu{T}" /> not show this property
///     <para>You only need to use this if you ever create a config menu of this object</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DontShowItem : Attribute
{
}