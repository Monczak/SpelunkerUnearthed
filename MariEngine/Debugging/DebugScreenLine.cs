namespace MariEngine.Debugging;

public class DebugScreenLine
{
    public delegate string InfoRetriever();

    private readonly InfoRetriever retriever;

    public DebugScreenLine(InfoRetriever retriever)
    {
        this.retriever = retriever;
    }
    
    public string GetLine() => GetText();

    protected virtual string GetText()
    {
        return retriever();
    }
}

public class DebugScreenLine<T> : DebugScreenLine
{
    private T param;
    
    public new delegate string InfoRetriever(T param);
    private readonly InfoRetriever retriever;
    
    public DebugScreenLine(InfoRetriever retriever) : base(null)
    {
        this.retriever = retriever;
    }

    protected override string GetText()
    {
        return retriever(param);
    }

    public void SetParams(T param) => this.param = param;
}