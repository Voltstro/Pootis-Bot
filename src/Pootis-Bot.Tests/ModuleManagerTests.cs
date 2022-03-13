using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Pootis_Bot.Core;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Tests;

public class ModuleManagerTests
{
    private ModuleManager moduleManager;

    [OneTimeSetUp]
    public void Setup()
    {
        string directory = Path.GetFullPath(TestContext.CurrentContext.TestDirectory);
        Bot.ApplicationLocation = directory;
        moduleManager = new ModuleManager("Modules/", "Assemblies/");
    }

    [Test]
    public void ModuleManagerVerifyDependenciesCorrectTest()
    {
        List<Module> testModules = new()
        {
            new TestModule()
        };

        moduleManager.VerifyModuleDependencies(ref testModules, null);
    }

    [Test]
    public void ModuleManagerVerifyDependenciesCorrectMultipleTest()
    {
        List<Module> testModules = new()
        {
            new TestModule(),
            new DependentModule(),
            new DependCorrectModule()
        };

        moduleManager.VerifyModuleDependencies(ref testModules, null);
    }

    [Test]
    public void ModuleManagerVerifyDependenciesTwoOldTest()
    {
        List<Module> testModules = new()
        {
            new TestModule(),
            new DependentModule(),
            new DependTooOldModule()
        };

        moduleManager.VerifyModuleDependencies(ref testModules, null);
    }

    [Test]
    public void ModuleManagerVerifyDependenciesTwoNewTest()
    {
        List<Module> testModules = new()
        {
            new TestModule(),
            new DependentModule(),
            new DependTooNewModule()
        };

        moduleManager.VerifyModuleDependencies(ref testModules, null);
    }

    private class TestModule : Module
    {
        protected override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("TestModule", "Voltstro", new Version(1, 0));
        }
    }

    private class DependentModule : Module
    {
        protected override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("DependentModule", "Voltstro", new Version(2, 0));
        }
    }

    private class DependCorrectModule : Module
    {
        protected override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("DependCorrectModule", "Voltstro", new Version(1, 0),
                new ModuleDependency("DependentModule", new Version(2, 0)));
        }
    }

    private class DependTooOldModule : Module
    {
        protected override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("DependCorrectModule", "Voltstro", new Version(1, 0),
                new ModuleDependency("DependentModule", new Version(3, 0)));
        }
    }

    private class DependTooNewModule : Module
    {
        protected override ModuleInfo GetModuleInfo()
        {
            return new ModuleInfo("DependCorrectModule", "Voltstro", new Version(1, 0),
                new ModuleDependency("DependentModule", new Version(1, 0)));
        }
    }
}