using System.Diagnostics.CodeAnalysis;

using Indexer.Collections.Generic;
using Indexer.ViewModel;

namespace Indexer.Collections
{
    public class LabelVMObservableCollection
        : ObservableKeyedCollection<string, LabelViewModel>
    {
        protected override string GetKeyForItem([NotNull] LabelViewModel item)
        {
            return item.Name;
        }
    }
}
