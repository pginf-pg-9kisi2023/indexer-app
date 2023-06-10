using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Indexer.Collections;
using Indexer.Model;

namespace Indexer.ViewModel
{
    public class IndexedImageViewModel : ViewModelBase
    {
        public ImageViewModel Image { get; private set; }
        private readonly Session _session;
        private readonly IndexedImage _indexedImage;
        public string ImagePath => _indexedImage.ImagePath;
        public LabelVMObservableCollection Labels { get; private set; } = new();

        public IndexedImageViewModel(Session session, IndexedImage indexedImage)
        {
            _session = session;
            _indexedImage = indexedImage;
            Image = new(ImagePath);
            Label? label = null;
            Labels.AddRange(
                from hint in _session.Config.Hints
                select new LabelViewModel(
                    hint,
                    indexedImage.Labels.TryGetValue(hint.Name, out label) ? label : null
                )
            );
        }

        public LabelViewModel AddLabel([NotNull] Label label)
        {
            _indexedImage.AddLabel(label);
            var ret = Labels[label.Name];
            ret._label = label;
            return ret;
        }

        public void DeleteLabel(string labelName)
        {
            _indexedImage.DeleteLabel(labelName);
            Labels[labelName]._label = null;
        }
    }
}
