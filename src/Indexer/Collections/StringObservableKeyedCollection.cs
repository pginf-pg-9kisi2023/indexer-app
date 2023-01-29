using Indexer.Collections.Generic;

namespace Indexer.Collections
{
    public class StringObservableKeyedCollection
        : ObservableKeyedCollection<string, string>
    {
        protected override string GetKeyForItem(string item)
        {
            return item;
        }
    }
}
