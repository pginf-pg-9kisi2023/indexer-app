using System.Diagnostics.CodeAnalysis;

using Indexer.Collections.Generic;
using Indexer.Model;
using Indexer.ViewModel;

namespace Indexer.Collections
{
    public class IndexedImageVMObservableCollection
        : ObservableKeyedCollection<string, IndexedImageViewModel>
    {
        private ReadOnlyKeyedCollection<string, IndexedImage> indexedImages;

        public IndexedImageVMObservableCollection()
        {
        }

        public IndexedImageVMObservableCollection(ReadOnlyKeyedCollection<string, IndexedImage> indexedImages)
        {
            this.indexedImages = indexedImages;
        }

        protected override string GetKeyForItem([NotNull] IndexedImageViewModel item)
        {
            return item.ImagePath;
        }
    }
}
