using System;
using System.Linq;
using System.Reflection;
using MariEngine.Components;
using MariEngine.Loading;
using MariEngine.Rendering;
using MariEngine.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MariEngine.Services;

public class SceneLoader : ResourceLoaderService<PackedScene, SceneData>
{
    protected override string ContentPath => ContentPaths.Scenes;

    public override void LoadContent(INamingConvention namingConvention = null, IDeserializer deserializer = null)
    {
        base.LoadContent(deserializer: new DeserializerBuilder()
            .WithNamingConvention(namingConvention ?? PascalCaseNamingConvention.Instance)
            .WithTypeConverter(new Coord.YamlConverter())
            .WithTypeDiscriminatingNodeDeserializer(o =>
            {
                var componentKeyMappings = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && (TypeUtils.IsAssignableToGenericType(t, typeof(Component<>)) || TypeUtils.IsAssignableToGenericType(t, typeof(TileEntityComponent<>))))
                    .Select(t => (ComponentType: t, ProxyType: t.BaseType?.GetGenericArguments()[0]))
                    .ToDictionary(pair => pair.ComponentType.Name, pair => pair.ProxyType);
                componentKeyMappings.Add("Renderer", typeof(RendererData));
                
                o.AddKeyValueTypeDiscriminator<ComponentData>("ProxyType", componentKeyMappings);
            })
            .Build());
    }
}