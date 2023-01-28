using System.Collections.ObjectModel;

using Indexer.Model;

namespace Indexer.Collections
{
    internal class LabelCollection : KeyedCollection<string, Label>
    {
        protected override string GetKeyForItem(Label item)
        {
            return item.Name;
        }
    }
}
