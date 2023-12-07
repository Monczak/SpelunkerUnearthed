using SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public record struct BiomeData(string Name,
    string Color,
    ParameterProviderData WallProvider,
    ParameterProviderData GroundProvider,
    ParameterProviderData FillProvider,
    ParameterProviderData SmoothProvider);