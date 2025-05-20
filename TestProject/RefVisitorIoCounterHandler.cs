using System.Collections.Concurrent;
using System.Reflection;

namespace TestProject;

public class IoCounter
{
    public IoCounter(object @ref, int @in, int @out)
    {
        Ref = @ref;
        In = @in;
        Out = @out;
    }

    public readonly object Ref;
    public int In;
    public int Out;
}

public record RefVisitorIoCounterHandler(IoCounter? Owner, ConcurrentDictionary<object, IoCounter> ObjectCounter)
    : ISimpleRefVisitorContext.IHandler
{
    public void VisitNull()
    {
    }

    public void VisitArray(Array array, Action<ISimpleRefVisitorContext.IHandler> continuation)
    {
        IoCounter counter = ObjectCounter.GetOrAdd(array, key => new IoCounter(key, 0, 0));
        if (Owner is not null)
        {
            Interlocked.Increment(ref Owner.Out);
        }

        if (1 == Interlocked.Increment(ref counter.In))
        {
            continuation(new RefVisitorIoCounterHandler(counter, ObjectCounter));
        }
    }

    public void VisitObject(object obj, Action<ISimpleRefVisitorContext.IHandler> continuation)
    {
        IoCounter counter = ObjectCounter.GetOrAdd(obj, key => new IoCounter(key, 0, 0));
        if (Owner is not null)
        {
            Interlocked.Increment(ref Owner.Out);
        }

        if (1 == Interlocked.Increment(ref counter.In))
        {
            continuation(new RefVisitorIoCounterHandler(counter, ObjectCounter));
        }
    }

    public void VisitArrayElement(Array src, int[] index, Action<ISimpleRefVisitorContext.IHandler> continuation) =>
        continuation(this);

    public void VisitObjectField(object source, FieldInfo fieldInfo, Action<ISimpleRefVisitorContext.IHandler> continuation) =>
        continuation(this);
}