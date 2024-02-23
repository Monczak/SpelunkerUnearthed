using System;

namespace MariEngine.Persistence;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class SerializeCompressedAttribute : Attribute;