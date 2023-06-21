using System;
using System.Drawing;

using Indexer.Model;

namespace Indexer.ViewModel
{
    public class LabelViewModel : ViewModelBase
    {
        private readonly Hint _hint;
        private Label? __label;
        internal Label? _label
        {
            get => __label;
            set
            {
                __label = value;
                OnPropertyChanged(nameof(Position));
                OnPropertyChanged(nameof(X));
                OnPropertyChanged(nameof(XText));
                OnPropertyChanged(nameof(Y));
                OnPropertyChanged(nameof(YText));
            }
        }
        public string Name => _hint.Name;
        public Point? Position => _label is null ? null : new Point(X, Y);
        public string XText => X >= 0 ? $"{X}" : "---";
        public string YText => Y >= 0 ? $"{Y}" : "---";
        public int X
        {
            get => _label?.X ?? -1;
            set
            {
                if (value >= 0)
                {
                    if (_label is Label label)
                    {
                        label.X = value;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(XText));
                        OnPropertyChanged(nameof(Position));
                        return;
                    }
                }
                else if (_label is null)
                {
                    return;
                }

                throw new ArgumentException(
                    "X can only be assigned if object has a backing label."
                );
            }
        }
        public int Y
        {
            get => _label?.Y ?? -1;
            set
            {
                if (value >= 0)
                {
                    if (_label is Label label)
                    {
                        label.Y = value;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(YText));
                        OnPropertyChanged(nameof(Position));
                        return;
                    }
                }
                else if (_label is null)
                {
                    return;
                }

                throw new ArgumentException(
                    "Y can only be assigned if object has a backing label."
                );
            }
        }

        public LabelViewModel(Hint hint, Label? label)
        {
            _hint = hint;
            _label = label;
        }
    }
}
