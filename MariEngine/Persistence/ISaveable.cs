using System;
using System.IO;

namespace MariEngine.Persistence;

public interface ISaveable
{
    void Serialize(Stream stream);
}

public interface ISaveable<out T> : ISaveable
{
    static virtual T Deserialize(Stream stream) => throw new NotImplementedException();
}