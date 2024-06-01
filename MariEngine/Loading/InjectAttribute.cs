using System;

namespace MariEngine.Loading;

[AttributeUsage(AttributeTargets.Parameter)]
public class InjectAttribute(string yamlAlias) : Attribute;