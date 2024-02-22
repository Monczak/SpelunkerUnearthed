using System.IO;
using System.Text;
using YamlDotNet.Serialization;

namespace MariEngine.Persistence;

public struct TestSaveable : ISaveable<TestSaveable>
{
    public int Number { get; init; }
    public string Text { get; init; }
    
    public void Serialize(Stream stream)
    {
        stream.Write(Encoding.Default.GetBytes(new SerializerBuilder().Build().Serialize(this)));
    }

    public static TestSaveable Deserialize(Stream stream)
    {
        var reader = new StreamReader(stream);
        return new DeserializerBuilder().Build().Deserialize<TestSaveable>(reader.ReadToEnd());
    }
}