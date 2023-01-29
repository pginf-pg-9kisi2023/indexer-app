using Indexer.Model;

namespace Indexer.ViewModel
{
    public class HintViewModel : ViewModelBase
    {
        public ImageViewModel Image { get; private set; }
        private readonly Hint _hint;
        public string Name => _hint.Name;
        public string Description => _hint.Description;
        public string ImagePath => _hint.ImagePath;

        public HintViewModel(Hint hint)
        {
            _hint = hint;
            Image = new(ImagePath);
        }
    }
}
