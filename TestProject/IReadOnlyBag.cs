namespace TestProject;

public interface IReadOnlyBag<T> : IEnumerable<KeyValuePair<T, int>>
{
    int Get(T item);
}