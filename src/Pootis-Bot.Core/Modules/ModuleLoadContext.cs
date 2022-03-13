using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules;

/// <summary>
///     Handles loading the <see cref="Assembly" /> for modules
/// </summary>
internal sealed class ModuleLoadContext : AssemblyLoadContext
{
    private readonly string assembliesPath;
    private readonly string modulesPath;

    /// <summary>
    ///     Creates a new <see cref="ModuleLoadContext" /> instance
    /// </summary>
    /// <param name="modulesPath">The path of the modules</param>
    /// <param name="assembliesPath">The path of the assemblies required by the modules</param>
    internal ModuleLoadContext(string modulesPath, string assembliesPath)
    {
        this.modulesPath = modulesPath;
        this.assembliesPath = assembliesPath;
        Resolving += OnResolving;
    }

    private Assembly OnResolving(AssemblyLoadContext loadContext, AssemblyName assemblyName)
    {
        //Try and load it from the assembly path first
        if (File.Exists($"{assembliesPath}/{assemblyName.Name}.dll"))
            return loadContext.LoadFromAssemblyPath($"{assembliesPath}/{assemblyName.Name}.dll");

        //Try and load it from the modules path
        if (File.Exists($"{modulesPath}/{assemblyName.Name}.dll"))
            return loadContext.LoadFromAssemblyPath($"{modulesPath}/{assemblyName.Name}.dll");

        //Try and load it from the root dir
        if (File.Exists($"{Bot.ApplicationLocation}/{assemblyName.Name}.dll"))
            return loadContext.LoadFromAssemblyPath($"{Bot.ApplicationLocation}/{assemblyName.Name}.dll");

        return null;
    }
}