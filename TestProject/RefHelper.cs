using System.Collections.Immutable;
using System.Reflection;

namespace TestProject;

/// <summary>
/// Tests with recursive reversed array filling, stackalloc, span, or just a single allocation.
/// </summary>

public static class RefHelper
{
    /// <summary>
    /// Create inheritance members array.
    /// </summary>
    /// <param name="type"> type reference</param>
    /// <returns> <c>Type[] {typeof(Object), ... </c><paramref name="type"/><c>}</c></returns>
    public static Type[] InheritanceArray(Type type)
    {
        return InheritanceArray(type, 0);
    }

    private static Type[] InheritanceArray(Type type, int level)
    {
        var nextLevel = level + 1;
        var baseType = type.BaseType;
        var result = (null == baseType)
            ? new Type [nextLevel]
            : InheritanceArray(baseType, nextLevel);
        result[^nextLevel] = type;
        return result;
    }

    /// <summary>
    /// Create inheritance members list.
    /// </summary>
    /// <param name="type"> type reference</param>
    /// <returns> <c>List&lt;Type> {typeof(Object), ... </c><paramref name="type"/><c>}</c></returns>
    public static IReadOnlyList<Type> InheritanceList(Type type)
    {
        return InheritanceList(type, 0);
    }

    private static List<Type> InheritanceList(Type type, int level)
    {
        var nextLevel = level + 1;
        var baseType = type.BaseType;
        var result = (null == baseType)
            ? new List<Type>(nextLevel)
            : InheritanceList(baseType, nextLevel);
        result[^nextLevel] = type;
        return result;
    }

    /// <summary>
    /// Create all inherited field array.
    /// </summary>
    /// <param name="type"> type reference</param>
    /// <returns> field array </returns>
    public static FieldInfo[] FieldsArray(Type type)
    {
        return FieldsArray(type, 0);
    }

    private static FieldInfo[] FieldsArray(Type type, int level)
    {
        var declared = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        var declaredLength = declared.Length;
        var nextLevel = level + declared.Length;
        var baseType = type.BaseType;
        var result = (null == baseType)
            ? new FieldInfo[nextLevel]
            : FieldsArray(baseType, nextLevel);
        if (0 < declaredLength)
        {
            declared.CopyTo(result, result.Length - nextLevel);
        }

        return result;
    }

    /// <summary>
    /// Create all inherited field list.
    /// </summary>
    /// <param name="type"> type reference</param>
    /// <returns> field array </returns>
    public static IReadOnlyList<FieldInfo> FieldsList(Type type)
    {
        return FieldsArray(type);
    }

    public static bool IsImmutable(Type type)
    {
        return false
               || type.IsAssignableTo(typeof(ValueType))
               //|| type.IsAssignableTo(typeof(IImmutableList<>))
               //|| type.IsAssignableTo(typeof(IImmutableSet<>))
               //|| type.IsAssignableTo(typeof(IImmutableDictionary<,>))
            ;
    }

    public static T CreateObjectInstance<T>()
    {
        var type = typeof(T);
        var instance = type.Assembly.CreateInstance(
            type.FullName!, false,
            BindingFlags.Instance | BindingFlags.NonPublic,
            null, [], null, null);
        return (T)instance;
    }

    public static T CreateArrayInstance<T>()
    {
        var type = typeof(T);
        var instance = type.Assembly.CreateInstance(
            type.FullName ?? type.Name, false,
            BindingFlags.Instance | BindingFlags.NonPublic,
            null, [], null, null);
        if (null == instance)
        {
            throw new Exception("Could not create instance of type " + type.FullName);
        }
        else
        {
            return (T)instance;
        }
    }

    public static int[] GetDimensions(this Array src)
    {
        int rank = src.Rank;
        int[] result = new int[rank];
        for (int i = 0; i < rank; i++)
        {
            result[i] = src.GetLength(i);
        }

        return result;
    }

    public static void WithDimensions(this Array src, Action<ReadOnlySpan<int>> apply)
    {
        int rank = src.Rank;
        Span<int> dimensions = stackalloc int[rank];
        for (int i = 0; i < rank; i++)
        {
            dimensions[i] = src.GetLength(i);
        }

        apply(dimensions);
    }

    public static T WithDimensions<T>(this Array src, Func<ReadOnlySpan<int>, T> apply)
    {
        int rank = src.Rank;
        Span<int> dimensions = stackalloc int[rank];
        for (int i = 0; i < rank; i++)
        {
            dimensions[i] = src.GetLength(i);
        }

        return apply(dimensions);
    }

    public static T WithDimensionsArray<T>(this Array src, Func<Array, ReadOnlySpan<int>, T> apply)
    {
        int rank = src.Rank;
        Span<int> dimensions = stackalloc int[rank];
        for (int i = 0; i < rank; i++)
        {
            dimensions[i] = src.GetLength(i);
        }

        return apply(src, dimensions);
    }

    public static void ForEachArrayDimension(ReadOnlySpan<int> dimensions, Action<ReadOnlySpan<int>> action)
    {
        int dimensionsLength = dimensions.Length;
        Span<int> index = stackalloc int[dimensionsLength];
        ForEachDimension(0, dimensionsLength - 1, dimensions, index, action);
    }

    private static void ForEachDimension(int level, int left, ReadOnlySpan<int> dimensions, Span<int> index,
        Action<ReadOnlySpan<int>> action)
    {
        int dim = dimensions[level];
        if (0 == left)
        {
            for (int i = 0; i < dim; ++i)
            {
                index[level] = i;
                action(index);
            }
        }
        else
        {
            for (int i = 0; i < dim; ++i)
            {
                index[level] = i;
                ForEachDimension(level + 1, left - 1, dimensions, index, action);
            }
        }
    }

    public static void ForEachDimension(this Array src, Action<int[]> action) =>
        ForEachArrayDimension(src.GetDimensions(), action);

    public static void ForEachArrayDimension(int[] dimensions, Action<int[]> action)
    {
        int dimensionsLength = dimensions.Length;
        int[] index = new int[dimensionsLength];
        ForEachDimension(0, dimensionsLength - 1, dimensions, index, action);
    }

    private static void ForEachDimension(int level, int left, int[] dimensions, int[] index,
        Action<int[]> action)
    {
        int dim = dimensions[level];
        if (0 == left)
        {
            for (int i = 0; i < dim; ++i)
            {
                index[level] = i;
                action(index);
            }
        }
        else
        {
            for (int i = 0; i < dim; ++i)
            {
                index[level] = i;
                ForEachDimension(level + 1, left - 1, dimensions, index, action);
            }
        }
    }

    private static void ForEachDimension(Array src, int level, int left, Span<int> index, Action<ReadOnlySpan<int>> action)
    {
        int dim = src.GetLength(level);
        if (0 == left)
        {
            for (int i = 0; i < dim; ++i)
            {
                index[level] = i;
                action(index);
            }
        }
        else
        {
            for (int i = 0; i < dim; ++i)
            {
                index[level] = i;
                ForEachDimension(src, level + 1, left - 1, index, action);
            }
        }
    }

    public static void ForEachDimension(Array src, Action<ReadOnlySpan<int>> action)
    {
        int dimensionsLength = src.Rank;
        Span<int> index = stackalloc int[dimensionsLength];
        ForEachDimension(src, 0, dimensionsLength - 1, index, action);
    }

    // public static void ForEachDimension(this Array src, Action<ReadOnlySpan<int>> action) =>
    //     WithDimensions(src, dimensions => ForEachArrayDimension(dimensions, action));

    public static void ForEachDimension(this Array src, Action<Array, ReadOnlySpan<int>> action)
    {
        ForEachDimension(src, (ReadOnlySpan<int> index) => action(src, index));
    }
}