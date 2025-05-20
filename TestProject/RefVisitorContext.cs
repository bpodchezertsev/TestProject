using System.Collections.Concurrent;
using System.Reflection;

namespace TestProject;

/// <summary>
/// Cascaded Reflection Visitor with circular reference avoiding. 
/// </summary>
public interface IRefVisitorContext
{
    public interface IHandler
    {
        void VisitNull();
        void VisitArray(Array array, bool visited, Action<IHandler> continuation);
        void VisitObject(object obj, bool visited, Action<IHandler> continuation);
        void VisitArrayElement(Array src, int[] index, Action<IHandler> continuation);
        void VisitObjectField(object source, FieldInfo fieldInfo, Action<IHandler> continuation);
    }

    IReadOnlyList<FieldInfo> FieldsList(Type type);

    void VisitRef(object? src, IHandler handler);

    public static void ForEachRef(object? src, IHandler handler) =>
        ForEachRef(src, new ConcurrentDictionary<Type, IReadOnlyList<FieldInfo>>(), handler);

    public static void ForEachRef(object? src, IDictionary<Type, IReadOnlyList<FieldInfo>> fieldsCache, IHandler handler)
    {
        ConcurrentDictionary<object, int> visitedId = new ConcurrentDictionary<object, int>(ReferenceEqualityComparer.Instance);

        IRefVisitorContext context = new RefVisitorContext(fieldsCache, visitedId);
        context.VisitRef(src, handler);
    }

    public class RefVisitorContext : IRefVisitorContext
    {
        protected readonly ISimpleRefVisitorContext SimpleContext;
        protected readonly IDictionary<Type, IReadOnlyList<FieldInfo>> FieldsCache;
        protected readonly ConcurrentDictionary<object, int> VisitedId;
        protected IHandler? RootHandler;
        protected HandlerWrapper? RootHandlerWrapper;

        public RefVisitorContext(IDictionary<Type, IReadOnlyList<FieldInfo>> fieldsCache, ConcurrentDictionary<object, int> visitedId)
        {
            FieldsCache = fieldsCache;
            SimpleContext = new ISimpleRefVisitorContext.CachedSimpleRefVisitorContext(fieldsCache);
            VisitedId = visitedId;
        }

        public RefVisitorContext(IDictionary<Type, IReadOnlyList<FieldInfo>> fieldsCache)
            : this(fieldsCache, new ConcurrentDictionary<object, int>(ReferenceEqualityComparer.Instance))
        {
        }

        public RefVisitorContext()
            : this(new ConcurrentDictionary<Type, IReadOnlyList<FieldInfo>>())
        {
        }

        public IReadOnlyList<FieldInfo> FieldsList(Type type) =>
            FieldsCache.TryGetValue(type, out var result) ? result : throw new ConcurrentModificationException();

        public void VisitRef(object? src, IHandler handler)
        {
            if (RootHandler is null) // first call
            {
                RootHandler = handler;
                RootHandlerWrapper = new HandlerWrapper(VisitedId, handler);
                SimpleContext.VisitRef(src, RootHandlerWrapper);
            }
            else if (ReferenceEquals(RootHandler, handler))
            {
                SimpleContext.VisitRef(src, RootHandlerWrapper!);
            }
            else
            {
                SimpleContext.VisitRef(src, new HandlerWrapper(VisitedId, handler));
            }
        }
    }

    public record HandlerWrapper(ConcurrentDictionary<object, int> VisitedId, IHandler Handler) : ISimpleRefVisitorContext.IHandler
    {
        public void VisitNull() => Handler.VisitNull();

        public void VisitArray(Array array, Action<ISimpleRefVisitorContext.IHandler> continuation)
        {
            int id = VisitedId.AddOrUpdate(array, 1, (k, old) => old <= 0 ? 1 : 2);
            if (1 == id)
            {
                Handler.VisitArray(array, false, handler =>
                    continuation(ReferenceEquals(Handler, handler) ? this : new HandlerWrapper(VisitedId, handler))
                );
            }
            else
            {
                Handler.VisitArray(array, true, _ => { });
            }
        }

        public void VisitObject(object obj, Action<ISimpleRefVisitorContext.IHandler> continuation)
        {
            int id = VisitedId.AddOrUpdate(obj, 1, (k, old) => old <= 0 ? 1 : 2);
            if (1 == id)
            {
                Handler.VisitObject(obj, false, handler =>
                    continuation(ReferenceEquals(Handler, handler) ? this : new HandlerWrapper(VisitedId, handler))
                );
            }
            else
            {
                Handler.VisitObject(obj, true, _ => { });
            }
        }

        public void VisitArrayElement(Array src, int[] index, Action<ISimpleRefVisitorContext.IHandler> continuation) =>
            Handler.VisitArrayElement(src, index, handler => continuation(new HandlerWrapper(VisitedId, handler)));

        public void VisitObjectField(object source, FieldInfo fieldInfo, Action<ISimpleRefVisitorContext.IHandler> continuation) =>
            Handler.VisitObjectField(source, fieldInfo, handler => continuation(new HandlerWrapper(VisitedId, handler)));
    }
}