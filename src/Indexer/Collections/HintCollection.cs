using System.Collections.ObjectModel;

using Indexer.Model;

namespace Indexer.Collections
{
    internal class HintCollection : KeyedCollection<string, Hint>
    {
        protected override string GetKeyForItem(Hint item)
        {
            return item.Name;
        }
    }
}
