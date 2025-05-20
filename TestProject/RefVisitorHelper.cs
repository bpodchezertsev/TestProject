using System.Reflection;
using System.Runtime.CompilerServices;

namespace TestProject;

public static class RefVisitorHelper
{
    public class WriteStructHandler(TextWriter output) : IRefVisitorContext.IHandler
    {
        public void VisitNull()
        {
            output.Write("null");
        }

        public void VisitArray(Array array, bool visited, Action<IRefVisitorContext.IHandler> continuation)
        {
            output.Write(array.GetType().GetElementType().FullName ?? array.GetType().GetElementType().Name);
            output.Write("[");
            RefHelper.WithDimensions(array, index =>
                index.ForAllThenLastOne(v =>
                {
                    output.Write(v);
                    output.Write(", ");
                }, output.Write));
            output.Write("]");
            output.Write(" {");
            output.Write("<");
            output.Write(RuntimeHelpers.GetHashCode(array));
            output.Write(">");
            continuation(this);
            output.Write("}");
        }

        public void VisitObject(object obj, bool visited, Action<IRefVisitorContext.IHandler> continuation)
        {
            output.Write(obj.GetType().FullName ?? obj.GetType().Name);
            output.Write("{");
            output.Write("<");
            output.Write(RuntimeHelpers.GetHashCode(obj));
            output.Write(">");
            continuation(this);
            output.Write("}");
        }

        public void VisitArrayElement(Array src, int[] index, Action<IRefVisitorContext.IHandler> continuation)
        {
            continuation(this);
            output.Write(", ");
        }

        public void VisitObjectField(object source, FieldInfo fieldInfo, Action<IRefVisitorContext.IHandler> continuation)
        {
            output.Write(fieldInfo.Name);
            output.Write(": ");
            continuation(this);
            output.Write(", ");
        }
    };

    public static void WriteStruct(TextWriter output, object? src)
    {
        WriteStructHandler handler = new WriteStructHandler(output);
        IRefVisitorContext.ForEachRef(src, handler);
    }
}