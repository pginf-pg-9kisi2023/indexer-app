using System.Diagnostics.CodeAnalysis;

using Indexer.Collections.Generic;
using Indexer.ViewModel;

namespace Indexer.Collections
{
    public class IndexedImageVMObservableCollection
        : ObservableKeyedCollection<string, IndexedImageViewModel>
    {
        protected override string GetKeyForItem([NotNull] IndexedImageViewModel item)
        {
            return item.ImagePath;
        }
    }
}
