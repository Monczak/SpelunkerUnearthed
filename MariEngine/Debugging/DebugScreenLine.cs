namespace MariEngine.Debugging;

public class DebugScreenLine(DebugScreenLine.InfoRetriever retriever)
{
    public delegate string InfoRetriever();

    public string GetLine() => GetText();

    protected virtual string GetText()
    {
        return retriever();
    }
}

public class DebugScreenLine<T>(DebugScreenLine<T>.InfoRetriever retriever) : DebugScreenLine(null)
{
    private T param;
    
    public new delegate string InfoRetriever(T param);

    protected override string GetText()
    {
        return retriever(param);
    }

    public void SetParams(T param) => this.param = param;
}