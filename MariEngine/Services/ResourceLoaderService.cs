using System;
using System.Collections.Generic;
using System.IO;
using MariEngine.Exceptions;
using MariEngine.Loading;
using MariEngine.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MariEngine.Services;

public abstract class ResourceLoaderService<TItem, TProxyData> : Service where TItem : Resource<TProxyData>, new()
{
    protected abstract string ContentPath { get; }

    public Dictionary<string, TItem> Content { get; private set; }

    public virtual void LoadContent(INamingConvention namingConvention = null)
    {
        Content = new Dictionary<string, TItem>();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(namingConvention ?? PascalCaseNamingConvention.Instance)
            .Build();
        
        foreach (var file in Directory.GetFiles(ContentPath))
        {
            string id = Path.GetFileNameWithoutExtension(file);
            try
            {
                var data = deserializer.Deserialize<TProxyData>(File.ReadAllText(file));
                var item = ResourceBuilder.Build<TItem, TProxyData>(id, data);
                if (Content.ContainsKey(id))
                {
                    throw new ContentLoadingException($"Resource of type {typeof(TItem).Name} with ID {id} already exists.");
                }
                Content[id] = item;
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not load resource of type {typeof(TItem).Name} with ID {id}: {e.GetType().Name}: {e.Message}");
            }
        }
        
        Logger.Log($"Loaded {Content.Count} resources of type {typeof(TItem).Name}");
    }

    public TItem Get(string id) => Content[id];
}