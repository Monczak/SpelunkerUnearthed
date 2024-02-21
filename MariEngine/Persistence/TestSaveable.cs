using System.Text;
using YamlDotNet.Serialization;

namespace MariEngine.Persistence;

public struct TestSaveable : ISaveable<TestSaveable>
{
    public int Number { get; init; }
    public string Text { get; init; }
    
    public byte[] Serialize()
    {
        return Encoding.Default.GetBytes(new SerializerBuilder().Build().Serialize(this));
    }

    public static TestSaveable Deserialize(byte[] data)
    {
        return new DeserializerBuilder().Build().Deserialize<TestSaveable>(Encoding.Default.GetString(data));
    }
}