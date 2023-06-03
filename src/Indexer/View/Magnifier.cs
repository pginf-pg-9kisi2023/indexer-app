using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Indexer.ViewModel;

using Point = System.Drawing.Point;

namespace Indexer.View
{
    public class Magnifier : DockPanel
    {
        public static readonly DependencyProperty ZoomFactorProperty
            = DependencyProperty.Register(
                "ZoomFactor",
                typeof(double),
                typeof(Magnifier),
                new PropertyMetadata(default(double))
            );
        public static readonly DependencyProperty StrokeProperty
            = DependencyProperty.Register(
                "Stroke",
                typeof(SolidColorBrush),
                typeof(Magnifier),
                new PropertyMetadata(Brushes.Black)
            );

        public static readonly DependencyProperty ImageBitmapProperty
            = DependencyProperty.Register(
                "ImageBitmap",
                typeof(ImageSource),
                typeof(Magnifier),
                new PropertyMetadata(default(ImageSource), OnImageBitmapChange)
            );

        public static readonly DependencyProperty CurrentLabelProperty
            = DependencyProperty.Register(
                "CurrentLabel",
                typeof(LabelViewModel),
                typeof(Magnifier),
                new PropertyMetadata(default(LabelViewModel), OnCurrentLabelChange)
            );
        public static readonly DependencyProperty ImageCursorProperty
            = DependencyProperty.Register(
                "ImageCursor",
                typeof(Point?),
                typeof(Magnifier),
                new PropertyMetadata(default(Point?), OnImageCursorChange)
            );
        public SolidColorBrush Stroke
        {
            get => (SolidColorBrush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }
        public LabelViewModel? CurrentLabel
        {
            get => (LabelViewModel)GetValue(CurrentLabelProperty);
            set => SetValue(CurrentLabelProperty, value);
        }
        public Point? ImageCursor
        {
            get => (Point?)GetValue(ImageCursorProperty);
            set => SetValue(ImageCursorProperty, value);
        }
        public BitmapSource ImageBitmap
        {
            get => (BitmapSource)GetValue(ImageBitmapProperty);
            set => SetValue(ImageBitmapProperty, value);
        }
        public double ZoomFactor
        {
            get => (double)GetValue(ZoomFactorProperty);
            set => SetValue(ZoomFactorProperty, value);
        }

        protected Rect ViewBox
        {
            get => MagnifierImageBrush.Viewbox;
            set => MagnifierImageBrush.Viewbox = value;
        }
        protected ImageBrush MagnifierImageBrush { get; set; } = new();
        protected Rectangle MagnifierRectangle { get; set; } = new();

        public Magnifier()
        {
            TriggerMagnifierUpdate();
        }

        private static void OnImageBitmapChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            var self = (Magnifier)sender;
            if (self is null)
            {
                return;
            }
            self.TriggerMagnifierUpdate();
        }

        private void TriggerMagnifierUpdate()
        {
            MagnifierImageBrush = new ImageBrush(ImageBitmap)
            {
                ViewboxUnits = BrushMappingMode.Absolute
            };

            MagnifierRectangle.SizeChanged -= OnRectangleSizeChange;
            MagnifierRectangle = new Rectangle
            {
                Stroke = Stroke,
                Width = Width,
                Height = Height,
                Visibility = Visibility.Visible,
                Fill = MagnifierImageBrush
            };
            MagnifierRectangle.SizeChanged += OnRectangleSizeChange;

            Children.Clear();
            Children.Add(MagnifierRectangle);
        }

        private static void OnImageCursorChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            var self = (Magnifier)sender;
            if (self is null)
            {
                return;
            }

            self.TriggerViewBoxUpdate();
        }

        private static void OnCurrentLabelChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            var self = (Magnifier)sender;
            if (self is null)
            {
                return;
            }

            self.TriggerViewBoxUpdate();
            if (e.OldValue is LabelViewModel oldValue)
            {
                oldValue.PropertyChanged -= self.OnLabelPropertyChanged;
            }
            if (e.NewValue is LabelViewModel newValue)
            {
                newValue.PropertyChanged += self.OnLabelPropertyChanged;
            }
        }

        private void OnLabelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y")
            {
                TriggerViewBoxUpdate();
            }
        }

        private void OnRectangleSizeChange(object sender, SizeChangedEventArgs e)
        {
            TriggerViewBoxUpdate(resetViewBox: true);
        }

        private void TriggerViewBoxUpdate(bool resetViewBox = false)
        {
            int x = 0;
            int y = 0;
            if (CurrentLabel != null)
            {
                x = CurrentLabel.X;
                y = CurrentLabel.Y;
            }
            else if (ImageCursor is Point pos)
            {
                x = pos.X;
                y = pos.Y;
            }
            else if (!resetViewBox)
            {
                // keep the current viewbox
                return;
            }
            var width = MagnifierRectangle.ActualWidth * (1 / ZoomFactor);
            var height = MagnifierRectangle.ActualHeight * (1 / ZoomFactor);
            ViewBox = new Rect(x - width / 2, y - height / 2, width, height);
        }
    }
}
