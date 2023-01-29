using System.Linq;

using Indexer.Collections;
using Indexer.Model;

namespace Indexer.ViewModel
{
    public class IndexedImageViewModel : ViewModelBase
    {
        public ImageViewModel Image { get; private set; }
        private readonly IndexedImage _indexedImage;
        public string ImagePath => _indexedImage.ImagePath;
        public LabelVMObservableCollection Labels { get; private set; } = new();

        public IndexedImageViewModel(IndexedImage indexedImage)
        {
            _indexedImage = indexedImage;
            Image = new(ImagePath);
            Labels.AddRange(
                from label in _indexedImage.Labels select new LabelViewModel(label)
            );
        }

        public LabelViewModel AddLabel(Label label)
        {
            _indexedImage.AddLabel(label);
            var ret = new LabelViewModel(label);
            Labels.Add(ret);
            return ret;
        }

        public void DeleteLabel(string labelName)
        {
            _indexedImage.DeleteLabel(labelName);
            Labels.Remove(labelName);
        }
    }
}
