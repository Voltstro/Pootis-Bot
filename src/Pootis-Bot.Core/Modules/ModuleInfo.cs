using System;
using System.Diagnostics.CodeAnalysis;

namespace Pootis_Bot.Modules;

/// <summary>
///     Info for a module
/// </summary>
public readonly struct ModuleInfo
{
    /// <summary>
    ///     Creates a new module info instance
    /// </summary>
    /// <param name="name">The name of the module</param>
    /// <param name="author">Who created this module</param>
    /// <param name="version">The version of the version</param>
    /// <param name="dependencies">Packages required by the module</param>
    public ModuleInfo([DisallowNull] string name, [DisallowNull] string author, [DisallowNull] Version version,
        params ModuleDependency[] dependencies)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentNullException(nameof(author));

        ModuleName = name;
        ModuleAuthorName = author;
        ModuleVersion = version ?? throw new ArgumentNullException(nameof(version));
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Creates a new module info instance
    /// </summary>
    /// <param name="name">The name of the module</param>
    /// <param name="author">Who created this module</param>
    /// <param name="version">The version of the version</param>
    public ModuleInfo([DisallowNull] string name, [DisallowNull] string author, [DisallowNull] Version version)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentNullException(nameof(author));

        ModuleName = name;
        ModuleAuthorName = author;
        ModuleVersion = version ?? throw new ArgumentNullException(nameof(version));
        Dependencies = Array.Empty<ModuleDependency>();
    }

    /// <summary>
    ///     The name of the module
    /// </summary>
    internal readonly string ModuleName;

    /// <summary>
    ///     Who created this module
    /// </summary>
    internal readonly string ModuleAuthorName;

    /// <summary>
    ///     The name of the module
    /// </summary>
    internal readonly Version ModuleVersion;

    /// <summary>
    ///     NuGet packages used by the module
    /// </summary>
    internal readonly ModuleDependency[] Dependencies;
}