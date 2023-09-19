using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Engine.Light;

public class LightMap : Component
{
    private Tilemap tilemap;
    public Color AmbientLight { get; set; }

    public override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public Color GetLight(Coord position)
    {
        Vector3 lightAtPoint = AmbientLight.ToVector3();
        foreach (TileEntity entity in tilemap.TileEntities)
        {
            LightSource lightSource = entity.GetComponent<LightSource>();
            if (lightSource is null) continue;
            
            Vector3 light = lightSource.GetLight(position).ToVector3();
            lightAtPoint += light;
        }

        float max = Math.Max(lightAtPoint.X, Math.Max(lightAtPoint.Y, lightAtPoint.Z));
        if (max >= 1)
            lightAtPoint /= max;

        return new Color(lightAtPoint);
    }
}