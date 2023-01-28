using System.Collections.ObjectModel;

using Indexer.Model;

namespace Indexer.Collections
{
    internal class IndexedImageCollection : KeyedCollection<string, IndexedImage>
    {
        protected override string GetKeyForItem(IndexedImage item)
        {
            return item.ImagePath;
        }
    }
}
