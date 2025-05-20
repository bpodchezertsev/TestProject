namespace TestProject;

public static class ExtHelper
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T element in source)
            action(element);
    }

    public static void ForEach<T>(this ReadOnlySpan<T> source, Action<T> action)
    {
        foreach (T element in source)
            action(element);
    }


    public static void ForEachRemaining<T>(this IEnumerator<T> source, Action<T> action)
    {
        while (source.MoveNext())
        {
            action(source.Current);
        }
    }

    public static void ForFirstOneThenAllOther<T>(this IEnumerable<T> source, Action<T> forFirstOne, Action<T> forAllOther)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
        {
            forFirstOne(enumerator.Current);
            enumerator.ForEachRemaining(forAllOther);
        }
    }

    public static void ForFirstOneThenAllOther<T>(this ReadOnlySpan<T> source, Action<T> forFirstOne, Action<T> forAllOther)
    {
        if (0 < source.Length)
        {
            forFirstOne(source[0]);
            for (int i = 1; i < source.Length; ++i)
            {
                forAllOther(source[i]);
            }
        }
    }

    public static void ForAllThenLastOne<T>(this IEnumerable<T> source, Action<T> forAll, Action<T> forLastOne)
    {
        using IEnumerator<T> enumerator = source.GetEnumerator();
        if (enumerator.MoveNext())
        {
            while (true)
            {
                T current = enumerator.Current;
                if (enumerator.MoveNext())
                {
                    forAll(current);
                }
                else
                {
                    forLastOne(current);
                    break;
                }
            }
        }
    }

    public static void ForAllThenLastOne<T>(this ReadOnlySpan<T> source, Action<T> forAll, Action<T> forLastOne)
    {
        if (0 < source.Length)
        {
            int i = 0;
            for (; i < source.Length - 1; ++i)
            {
                forAll(source[i]);
            }

            forLastOne(source[0]);
        }
    }

    public static Func<TInput, TResult> Memoize<TInput, TResult>(this Func<TInput, TResult> func) where TInput : notnull
    {
        return Memoize(func, new Dictionary<TInput, TResult>());
    }

    public static Func<TInput, TResult> Memoize<TInput, TResult>(this Func<TInput, TResult> func, IDictionary<TInput, TResult> memo)
    {
        return input =>
        {
            if (memo.TryGetValue(input, out var memoized))
                return memoized;

            var result = func(input);
            memo.Add(input, result);

            return result;
        };
    }
}