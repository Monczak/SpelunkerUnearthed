using System;
using System.Collections.Generic;
using System.IO;
using MariEngine.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MariEngine.Persistence;

public class SaveLoadContext : IDisposable
{
    private readonly string indexFilePath;
    private readonly DataNode root;

    private readonly Queue<(DataNode, ISaveable)> dataToSave = new();

    private string GetFilePath(DataNode node)
    {
        return Path.Combine(GetSaveFileDirectory(), node.AbsolutePath);
    }

    private string GetSaveFileDirectory()
    {
        var dirPath = Path.GetDirectoryName(indexFilePath);
        if (dirPath is null)
            throw new InvalidOperationException("Index file directory is null?");
        return dirPath;
    }

    public SaveLoadContext(string indexFilePath)
    {
        this.indexFilePath = indexFilePath;

        if (File.Exists(this.indexFilePath))
        {
            using var file = File.OpenText(this.indexFilePath);
            root = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build()
                .Deserialize<DataNode>(file.ReadToEnd());
            root.FixParents();
        }
        else
        {
            root = new DataNode("");
        }
    }
    
    public void Save<T>(ISaveable<T> data, string path)
    {
        if (root is null)
            throw new InvalidOperationException("No save file loaded.");

        var node = root.Get(path, createIfNotExists: true);
        dataToSave.Enqueue((node, data));
    }

    public T Load<T>(string path) where T : ISaveable<T>
    {
        if (root is null)
            throw new InvalidOperationException("No save file loaded.");

        var node = root.Get(path);
        if (node.IsGroup)
            throw new InvalidOperationException("The specified path does not point to a DataNode with saved data.");

        using var stream = File.OpenRead(GetFilePath(node));
        return T.Deserialize(stream);
    }

    public void Dispose()
    {
        var dirPath = GetSaveFileDirectory();
        Directory.CreateDirectory(dirPath);
        
        using (var file = File.CreateText(indexFilePath))
        {
            var indexData = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build()
                .Serialize(root);
            file.Write(indexData);
        }
        
        while (dataToSave.TryDequeue(out var dataInfo))
        {
            var (node, data) = dataInfo;
            using var serializedData = new MemoryStream();
            data.Serialize(serializedData);

            var nodePath = GetFilePath(node);
            Directory.CreateDirectory(Path.GetDirectoryName(nodePath) ?? throw new InvalidOperationException("Node directory was null?"));
            
            using var file = File.Create(nodePath);
            serializedData.Seek(0, SeekOrigin.Begin);
            serializedData.CopyTo(file);
        }
    }
}