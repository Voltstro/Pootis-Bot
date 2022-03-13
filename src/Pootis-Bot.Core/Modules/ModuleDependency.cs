using System;
using System.Diagnostics.CodeAnalysis;

namespace Pootis_Bot.Modules;

/// <summary>
///     A NuGet package required by a module
/// </summary>
public readonly struct ModuleDependency
{
    /// <summary>
    ///     Sets-up a dependency for a NuGet package
    /// </summary>
    /// <param name="packageId">The package id. (Whats it name on NuGet)</param>
    /// <param name="packageVersion">The version of the package</param>
    /// <param name="assemblyName">The name of the assembly that the package will extract to</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ModuleDependency([DisallowNull] string packageId, [DisallowNull] Version packageVersion,
        [DisallowNull] string assemblyName)
    {
        //Null check
        if (string.IsNullOrWhiteSpace(packageId))
            throw new ArgumentNullException(nameof(packageId));

        if (string.IsNullOrWhiteSpace(assemblyName))
            throw new ArgumentNullException(nameof(assemblyName));

        PackageId = packageId;
        PackageVersion = packageVersion ?? throw new ArgumentNullException(nameof(packageVersion));
        AssemblyName = assemblyName;
        ModuleName = null;
        ModuleMinVersion = null;
    }

    /// <summary>
    ///     Sets-up a dependency for another module
    /// </summary>
    /// <param name="moduleName">The name of the module to depend on</param>
    /// <param name="minVersion"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ModuleDependency([DisallowNull] string moduleName, [DisallowNull] Version minVersion)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentNullException(nameof(moduleName));

        PackageId = null;
        PackageVersion = null;
        AssemblyName = null;
        ModuleName = moduleName;
        ModuleMinVersion = minVersion;
    }

    /// <summary>
    ///     The package id. (Whats it name on NuGet)
    /// </summary>
    internal readonly string PackageId;

    /// <summary>
    ///     The package version
    /// </summary>
    internal readonly Version PackageVersion;

    /// <summary>
    ///     The name of the assembly that the package will extract to
    /// </summary>
    internal readonly string AssemblyName;

    /// <summary>
    ///     The module that a module depends on
    /// </summary>
    internal readonly string ModuleName;

    /// <summary>
    ///     Min expected module version
    /// </summary>
    internal readonly Version ModuleMinVersion;
}