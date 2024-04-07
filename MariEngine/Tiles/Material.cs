using MariEngine.Loading;

namespace MariEngine.Tiles;

public class Material : Resource<MaterialData>
{
    public float SoundReflectivity { get; private set; }
    public float SoundTransmittance { get; private set; }
    
    protected internal override void BuildFromData(MaterialData data)
    {
        SoundReflectivity = data.SoundReflectivity;
        SoundTransmittance = data.SoundTransmittance;
    }
}