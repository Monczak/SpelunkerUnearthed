using System;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public class ParameterProviderNameAttribute : Attribute
{
    public string Name { get; init; }

    public ParameterProviderNameAttribute(string name)
    {
        Name = name;
    }
}