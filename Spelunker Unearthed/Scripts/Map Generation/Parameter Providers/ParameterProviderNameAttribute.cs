using System;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public class ParameterProviderNameAttribute(string name) : Attribute
{
    public string Name { get; init; } = name;
}