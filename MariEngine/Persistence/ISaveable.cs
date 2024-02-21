using System;

namespace MariEngine.Persistence;

public interface ISaveable
{
    byte[] Serialize();
}

public interface ISaveable<out T> : ISaveable
{
    static virtual T Deserialize(byte[] data) => throw new NotImplementedException();
}