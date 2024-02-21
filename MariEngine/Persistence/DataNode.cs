using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace MariEngine.Persistence;

[Serializable]
internal class DataNode
{
    public string Name { get; private set; }
    [YamlIgnore] public bool IsGroup => Children.Count > 0;
    
    public Dictionary<string, DataNode> Children { get; private set; } = [];

    private DataNode Parent { get; set; }

    [YamlIgnore]
    public string AbsolutePath
    {
        get
        {
            var pathToNode = new LinkedList<string>();
            var currentNode = this;
            while (currentNode.Parent is not null)
            {
                pathToNode.AddFirst(currentNode.Name);
                currentNode = currentNode.Parent;
            }

            return string.Join("/", pathToNode);
        }
    }

    public DataNode()
    {
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataNode(string name)
    {
        Name = name;
    }
    
    private void AddChild(DataNode node)
    {
        Children.Add(node.Name, node);
        node.Parent = this;
    }

    public DataNode Get(string childPath, bool createIfNotExists = false)
    {
        var subpaths = childPath.Split("/");
        return Get(subpaths, createIfNotExists);
    }

    private DataNode Get(string[] subpaths, bool createIfNotExists)
    {
        if (subpaths.Length == 0)
            return this;

        var child = Children.GetValueOrDefault(subpaths[0]);
        if (child is null)
        {
            if (!createIfNotExists) throw new ArgumentException("No node exists under this path.");
            child = new DataNode(subpaths[0]);
            AddChild(child);
        }

        return child.Get(subpaths[1..], createIfNotExists);
    }

    internal void FixParents()
    {
        foreach (var child in Children.Values)
        {
            child.Parent = this;
            child.FixParents();
        }
    }
}