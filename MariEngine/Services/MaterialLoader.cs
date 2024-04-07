using System.Collections.Generic;
using MariEngine.Loading;
using MariEngine.Tiles;
using YamlDotNet.Serialization;

namespace MariEngine.Services;

public class MaterialLoader : ResourceLoaderService<Material, MaterialData>
{
    private IEnumerable<Material> GetSpecialMaterials()
    {
        yield return ResourceBuilder.Build<Material, MaterialData>("None", new MaterialData
        {
            SoundReflectivity = 0,
            SoundTransmittance = 1
        });
    }
    
    protected override string ContentPath => ContentPaths.Materials;

    public override void LoadContent(INamingConvention namingConvention = null)
    {
        base.LoadContent(namingConvention);
        
        foreach (var material in GetSpecialMaterials())
            Content.Add(material.Id, material);
    }
}