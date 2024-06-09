using System;
using System.Collections.Generic;
using System.IO;
using MariEngine.Exceptions;
using MariEngine.Loading;
using MariEngine.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MariEngine.Services;


public abstract class ResourceLoaderService<TItem, TProxyData> : Service, IResourceLoaderService where TItem : Resource<TProxyData>, new()
{
    protected abstract string ContentPath { get; }

    public SortedDictionary<string, TItem> Content { get; private set; }

    public virtual void LoadContent(INamingConvention namingConvention = null)
    {
        Content = new SortedDictionary<string, TItem>();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(namingConvention ?? PascalCaseNamingConvention.Instance)
            .Build();
        
        foreach (var file in Directory.EnumerateFiles(ContentPath, "*.*"))
        {
            string id = Path.ChangeExtension(Path.GetRelativePath(ContentPath, file), null)
                .Replace("\\", "/");
            try
            {
                var data = deserializer.Deserialize<TProxyData>(File.ReadAllText(file));
                var item = ResourceBuilder.Build<TItem, TProxyData>(id, data);
                if (!Content.TryAdd(id, item))
                {
                    throw new ContentLoadingException($"Resource of type {typeof(TItem).Name} with ID {id} already exists.");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not load resource of type {typeof(TItem).Name} with ID {id}: {e.GetType().Name}: {e.Message}");
            }
        }
        
        Logger.Log($"Loaded {Content.Count} resources of type {typeof(TItem).Name}");
    }

    public TItem Get(string id) => Content[id];
    object IResourceLoaderService.Get(string id) => Content[id];
}

internal interface IResourceLoaderService
{
    public object Get(string id);
}