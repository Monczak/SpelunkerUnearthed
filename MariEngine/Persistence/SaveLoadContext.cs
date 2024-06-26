﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using MariEngine.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MariEngine.Persistence;

public class SaveLoadContext : IDisposable
{
    private readonly string indexFilePath;
    private readonly DataNode root;

    private readonly Queue<(DataNode, ISaveable)> dataToSave = new();

    private readonly record struct RootDataNodeProxy(DataNode Data, string GameVersion);

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
            var data = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build()
                .Deserialize<RootDataNodeProxy>(file.ReadToEnd());
            
            // TODO: Check save file game version (not MariEngine, but the assembly that uses MariEngine - use some IVersionProvider?)
            data.Data.FixParents();
            root = data.Data;
        }
        else
        {
            root = new DataNode("");
        }
    }

    public IEnumerable<string> GetHierarchy(PathElement path) => GetHierarchy(path.ToString());

    public IEnumerable<string> GetHierarchy(string path)
    {
        return root.Get(path).Children.Select(child => child.Key);
    }

    public void Save<T>(ISaveable<T> data, PathElement path) => Save(data, path.ToString());
    
    public void Save<T>(ISaveable<T> data, string path)
    {
        if (root is null)
            throw new InvalidOperationException("No save file loaded.");

        var node = root.Get(path, createIfNotExists: true);
        dataToSave.Enqueue((node, data));
    }

    public T Load<T>(PathElement path) where T : ISaveable<T> => Load<T>(path.ToString());

    public T Load<T>(string path) where T : ISaveable<T>
    {
        if (root is null)
            throw new InvalidOperationException("No save file loaded.");

        var node = root.Get(path);
        if (node.IsGroup)
            throw new InvalidOperationException("The specified path does not point to a DataNode with saved data.");

        using var stream = File.OpenRead(GetFilePath(node));
        if (SerializeCompressed(typeof(T)))
        {
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            return T.Deserialize(gzipStream);
        }

        return T.Deserialize(stream);
    }

    private static bool SerializeCompressed(Type type) => type.IsDefined(typeof(SerializeCompressedAttribute), true);

    public void Dispose()
    {
        SaveData();
    }

    private void SaveData()
    {
        var dirPath = GetSaveFileDirectory();
        Directory.CreateDirectory(dirPath);
        
        using (var file = File.CreateText(indexFilePath))
        {
            var data = new RootDataNodeProxy(root, "1.0.0"); // TODO: Get version information of the assembly that uses MariEngine
            var indexData = new SerializerBuilder()
                .EnsureRoundtrip()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build()
                .Serialize(data);
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

            if (SerializeCompressed(data.GetType()))
            {
                using var gzipStream = new GZipStream(file, CompressionMode.Compress);
                serializedData.CopyTo(gzipStream);
            }
            else
            {
                serializedData.CopyTo(file);
            }
        }
    }
}