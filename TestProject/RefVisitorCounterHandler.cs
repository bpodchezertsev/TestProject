using System.Reflection;

namespace TestProject;

public record RefVisitorCounterHandler(IBagAppender<object> ObjectCounter) : ISimpleRefVisitorContext.IHandler
{
    public void VisitNull()
    {
    }

    public void VisitArray(Array array, Action<ISimpleRefVisitorContext.IHandler> continuation)
    {
        if (1 == ObjectCounter.Add(array))
        {
            continuation(this);
        }
    }

    public void VisitObject(object obj, Action<ISimpleRefVisitorContext.IHandler> continuation)
    {
        if (1 == ObjectCounter.Add(obj))
        {
            continuation(this);
        }
    }

    public void VisitArrayElement(Array src, int[] index, Action<ISimpleRefVisitorContext.IHandler> continuation) =>
        continuation(this);

    public void VisitObjectField(object source, FieldInfo fieldInfo, Action<ISimpleRefVisitorContext.IHandler> continuation) =>
        continuation(this);
}