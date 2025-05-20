using System.Collections;
using System.Collections.Concurrent;

namespace TestProject;

public interface IBag<T> : IReadOnlyBag<T>, IBagAppender<T> where T : notnull
{
    void Remove(T item);
    void Clear(T item);

    public interface IBuilder : IReadOnlyBag<T>, IBagAppender<T>
    {
        IReadOnlyBag<T> ToReadOnlyBag();
    }

    public class Builder : IBuilder
    {
        private ConcurrentDictionary<T, Entry>? _itemCount =
            new ConcurrentDictionary<T, Entry>((IEqualityComparer<T>)(IEqualityComparer)ReferenceEqualityComparer.Instance);

        public class Entry(int count)
        {
            public int Count = count;

            protected bool Equals(Entry other) => Count == other.Count;

            public override bool Equals(object? obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Entry)obj);
            }

            public override int GetHashCode() => Count;
        };

        public int Add(T item)
        {
            if (_itemCount is null)
            {
                throw new AlreadyInitializedException("Builder result already built");
            }

            Entry result = _itemCount.GetOrAdd(item, k => new Entry(0));
            return Interlocked.Increment(ref result.Count);
        }

        public IReadOnlyBag<T> ToReadOnlyBag()
        {
            if (_itemCount is null)
            {
                throw new AlreadyInitializedException("Builder result already built");
            }

            ConcurrentDictionary<T, Entry> container = _itemCount;
            _itemCount = null;
            return new ReadOnlyFromBuilder(container);
        }

        public int Get(T item) => _itemCount.TryGetValue(item, out var entry) ? entry.Count : 0;

        public IEnumerator<KeyValuePair<T, int>> GetEnumerator() =>
            _itemCount.Select(e => KeyValuePair.Create(e.Key, e.Value.Count)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    public class Bag : IBag<T>
    {
        private readonly IDictionary<T, int> _itemCount =
            new Dictionary<T, int>((IEqualityComparer<T>)(IEqualityComparer)ReferenceEqualityComparer.Instance);

        public IEnumerator<KeyValuePair<T, int>> GetEnumerator() => _itemCount.GetEnumerator();

        public int Add(T item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_itemCount.TryGetValue(item, out var count))
            {
                _itemCount.Add(item, ++count);
                return count;
            }
            else
            {
                _itemCount.Add(item, 1);
                return 1;
            }
        }

        public void Remove(T item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_itemCount.TryGetValue(item, out var count))
            {
                if (0 == --count)
                {
                    _itemCount.Remove(item);
                }
                else
                {
                    _itemCount.Add(item, count);
                }
            }
        }

        public void Clear(T item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _itemCount.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Get(T item) => _itemCount.TryGetValue(item, out var count) ? count : 0;
    }

    public class ReadOnlyFromBuilder(IDictionary<T, Builder.Entry> itemCount) : IReadOnlyBag<T>
    {
        public int Get(T item) => itemCount.TryGetValue(item, out var entry) ? entry.Count : 0;

        public IEnumerator<KeyValuePair<T, int>> GetEnumerator() =>
            itemCount.Select(e => KeyValuePair.Create(e.Key, e.Value.Count)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}