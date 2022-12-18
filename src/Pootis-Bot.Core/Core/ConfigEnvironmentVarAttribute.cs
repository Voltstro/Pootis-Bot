using System;

namespace Pootis_Bot.Core;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigEnvironmentVarAttribute : Attribute
{
    public ConfigEnvironmentVarAttribute(string environmentVariable)
    {
        EnvironmentVariable = environmentVariable;
    }
    
    public string EnvironmentVariable { get; }
}