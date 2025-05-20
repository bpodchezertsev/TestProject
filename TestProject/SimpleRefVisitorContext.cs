using System.Reflection;

namespace TestProject;

/// <summary>
/// Simple Cascaded Reflection Visitor without checking circular references. 
/// </summary>
public interface ISimpleRefVisitorContext
{
    public interface IHandler
    {
        void VisitNull();
        void VisitArray(Array array, Action<IHandler> continuation);
        void VisitObject(object obj, Action<IHandler> continuation);
        void VisitArrayElement(Array src, int[] index, Action<IHandler> continuation);
        void VisitObjectField(object source, FieldInfo fieldInfo, Action<IHandler> continuation);
    }

    public void VisitRef(object? src, IHandler handler);

    public IReadOnlyList<FieldInfo> FieldsList(Type type);

    public static void ForEachRef(object? src, IHandler handler)
    {
        ISimpleRefVisitorContext context = new SimpleRefVisitorContext();
        context.VisitRef(src, handler);
    }

    public static void ForEachRefCached(object? src, IHandler handler)
    {
        ISimpleRefVisitorContext context = new CachedSimpleRefVisitorContext();
        context.VisitRef(src, handler);
    }

    public static void ForEachRefCached(IDictionary<Type, IReadOnlyList<FieldInfo>> cache, object? src, IHandler handler)
    {
        ISimpleRefVisitorContext context = new CachedSimpleRefVisitorContext(cache);
        context.VisitRef(src, handler);
    }

    public class SimpleRefVisitorContext : ISimpleRefVisitorContext
    {
        public void VisitRef(object? src, IHandler handler)
        {
            switch (src)
            {
                case null: handler.VisitNull(); break;
                case Array array:
                    handler.VisitArray(array, h1 =>
                    {
                        if (!array.GetType().GetElementType()!.IsPrimitive)
                            array.ForEachDimension(index =>
                                h1.VisitArrayElement(array, index, h2 =>
                                    VisitRef(array.GetValue(index), h2)));
                        else
                            array.ForEachDimension(index => h1.VisitArrayElement(array, index, _ => { }));
                    });
                    break;
                case object o:
                    handler.VisitObject(o, h1 =>
                        FieldsList(o.GetType()).ForEach(fieldInfo =>
                            h1.VisitObjectField(o, fieldInfo, h2 =>
                                {
                                    if (!fieldInfo.FieldType.IsPrimitive)
                                    {
                                        VisitRef(fieldInfo.GetValue(o), h2);
                                    }
                                }
                            )));
                    break;
            }
        }

        public virtual IReadOnlyList<FieldInfo> FieldsList(Type type)
        {
            return RefHelper.FieldsList(type);
        }
    };

    public class CachedSimpleRefVisitorContext : SimpleRefVisitorContext
    {
        private readonly Func<Type, IReadOnlyList<FieldInfo>> _memo;

        public CachedSimpleRefVisitorContext(IDictionary<Type, IReadOnlyList<FieldInfo>> cache) : base()
        {
            _memo = ExtHelper.Memoize(base.FieldsList, cache);
        }

        public CachedSimpleRefVisitorContext() : base()
        {
            _memo = ExtHelper.Memoize<Type, IReadOnlyList<FieldInfo>>(base.FieldsList);
        }

        public override IReadOnlyList<FieldInfo> FieldsList(Type type)
        {
            return _memo(type);
        }
    }
}