using System;
using MariEngine.Components;
using MariEngine.Loading;
using MariEngine.Utils;

namespace MariEngine.Light;

public class LightEmitter : TileEntityComponent<LightEmitterData>
{
    public LightSource LightSource { get; set; }
    private LightMap lightMap;

    public override void Build(LightEmitterData data)
    {
        LightSource = data.LightSource.Type switch
        {
            "Point" => new PointLight(ColorUtils.FromHex(data.LightSource.Color), data.LightSource.Intensity,
                data.LightSource.Radius),
            _ => throw new Exception($"{data.LightSource.Type} is not a valid light source type.")
        };
    }

    protected internal override void Initialize()
    {
        lightMap = OwnerEntity.Tilemap.GetComponent<LightMap>();
        lightMap.AddEmitter(this);
    }

    protected override void OnDestroy()
    {
        lightMap.RemoveEmitter(this);
    }

    protected internal override void OnPositionUpdate()
    {
        lightMap.UpdatePosition(LightSource, OwnerEntity.Position);
    }
}

public class LightEmitterData : ComponentData
{
    public class LightSourceData
    {
        public string Type { get; init; }
        public string Color { get; init; }
        public float Intensity { get; init; }
        public int Radius { get; init; }
    }
    
    public LightSourceData LightSource { get; init; }
}