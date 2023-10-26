using System;
using System.Collections.Generic;
using System.IO;
using MariEngine.Exceptions;
using MariEngine.Loading;
using MariEngine.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MariEngine.Services;

public abstract class LoaderService<TItem, TProxyData> : Service where TItem : LoadableObject<TProxyData>, new()
{
    protected abstract string ContentPath { get; }

    protected Dictionary<string, TItem> Content;

    public Dictionary<string, TItem> LoadContent(INamingConvention namingConvention = null)
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
                var item = LoadableObjectBuilder.Build<TItem, TProxyData>(id, data);
                if (Content.ContainsKey(id))
                {
                    throw new ContentLoadingException($"Item with ID {id} already exists.");
                }
                Content[id] = item;
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not load item {id}: {e.GetType().Name}: {e.Message}");
            }
        }
        
        Logger.Log($"{GetType().Name}: Loaded {Content.Count} items");

        return Content;
    }

    public TItem Get(string id) => Content[id];
}