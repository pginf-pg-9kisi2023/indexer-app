using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Indexer.Collections.Generic
{
    public interface IReadOnlyKeyedCollection<TKey, TItem> : IReadOnlyCollection<TItem>
    {
        public TItem this[TKey key] { get; }
        public bool Contains(TKey key);
        public bool TryGetValue(TKey key, out TItem? item);
    }

    public class ReadOnlyKeyedCollection<TKey, TItem>
        : ReadOnlyCollection<TItem>, IReadOnlyKeyedCollection<TKey, TItem>
        where TKey : notnull
    {
        private readonly KeyedCollection<TKey, TItem> _keyedCollection;
        public TItem this[TKey key] => _keyedCollection[key];

        public ReadOnlyKeyedCollection(KeyedCollection<TKey, TItem> keyedCollection)
            : base(keyedCollection)
        {
            _keyedCollection = keyedCollection;
        }

        public bool Contains(TKey key)
        {
            return _keyedCollection.Contains(key);
        }

        public bool TryGetValue(TKey key, out TItem? item)
        {
            return _keyedCollection.TryGetValue(key, out item);
        }
    }
}
