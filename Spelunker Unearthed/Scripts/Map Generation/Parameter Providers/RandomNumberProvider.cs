using MariEngine;
using MariEngine.Loading;
using MariEngine.Services;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public class RandomNumberProvider : NumberProvider
{
    private float min;
    private float max;
    private string rngId;
    
    public override float Get(Coord worldPos)
    {
        return ServiceRegistry.Get<RandomProvider>().Request(rngId).NextFloat(min, max);
    }

    protected override void BuildFromData(ParameterProviderData data)
    {
        ParseUtils.TryParseFloat(data.Options, "Min", out min);
        ParseUtils.TryParseFloat(data.Options, "Max", out max);

        ParseUtils.GetStringOrDefault(data.Options, "Rng", out rngId, required: false,
            defaultValue: Constants.GeneralPurposeRng);
    }
}