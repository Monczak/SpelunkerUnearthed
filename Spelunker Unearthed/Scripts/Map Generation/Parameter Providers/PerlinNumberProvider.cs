using MariEngine;
using MariEngine.Loading;
using MariEngine.Services;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public class PerlinNumberProvider : NumberProvider
{
    private Vector2 frequency;
    private float min;
    private float max;
    private string rngId;
    
    public override float Get(Coord worldPos)
    {
        return MathUtils.Lerp(min, max,
            ServiceRegistry.Get<RandomProvider>().Request(rngId).Perlin01((Vector2)worldPos * frequency));
    }

    protected override void BuildFromData(ParameterProviderData data)
    {
        if (!ParseUtils.TryParseVector2(data.Options, "Frequency", out frequency))
            throw new ResourceLoadingException("Could not parse Perlin noise frequency.");

        ParseUtils.TryParseFloat(data.Options, "Min", out min);
        ParseUtils.TryParseFloat(data.Options, "Max", out max);
        
        ParseUtils.GetStringOrDefault(data.Options, "Rng", out rngId, required: false,
            defaultValue: Constants.GeneralPurposeRng);
    }
}