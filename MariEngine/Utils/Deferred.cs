namespace MariEngine.Utils;

public class Deferred<T>
{
    private T current;
    private T actual;

    public T Get() => current;
    public void Set(T value) => actual = value;
    public void Update() => current = actual;

    public Deferred(T value)
    {
        current = value;
        actual = value;
    }
}