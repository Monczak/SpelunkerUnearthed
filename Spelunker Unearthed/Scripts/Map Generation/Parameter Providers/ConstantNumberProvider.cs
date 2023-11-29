using System;
using MariEngine;
using MariEngine.Loading;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public class ConstantNumberProvider : NumberProvider
{
    private float number;
    
    public override float Get(Coord worldPos)
    {
        return number;
    }

    protected override void BuildFromData(ParameterProviderData data)
    {
        ParseUtils.TryParseFloat(data.Options, "Value", out number);
    }
}