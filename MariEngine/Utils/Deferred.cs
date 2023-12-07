namespace MariEngine.Utils;

public class Deferred<T>(T value)
{
    private T actual = value;

    public T Get() => value;
    public void Set(T value) => actual = value;
    public void Update() => value = actual;
}