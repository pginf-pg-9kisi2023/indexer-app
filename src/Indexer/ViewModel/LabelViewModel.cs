using Indexer.Model;

namespace Indexer.ViewModel
{
    public class LabelViewModel : ViewModelBase
    {
        private readonly Label _label;
        public string Name => _label.Name;
        public int X
        {
            get => _label.X;
            set
            {
                _label.X = value;
                OnPropertyChanged();
            }
        }
        public int Y
        {
            get => _label.Y;
            set
            {
                _label.Y = value;
                OnPropertyChanged();
            }
        }

        public LabelViewModel(Label label)
        {
            _label = label;
        }
    }
}
