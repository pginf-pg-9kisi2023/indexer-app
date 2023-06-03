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
    public class Magnifier : Canvas
    {
        public static readonly DependencyProperty ZoomFactorProperty
            = DependencyProperty.Register(
                "ZoomFactor",
                typeof(double),
                typeof(Magnifier),
                new PropertyMetadata(default(double))
            );
        public static readonly DependencyProperty RadiusProperty
            = DependencyProperty.Register(
                "Radius",
                typeof(double),
                typeof(Magnifier),
                new PropertyMetadata(default(double))
            );
        public static readonly DependencyProperty ContentPanelProperty
            = DependencyProperty.Register(
                "ContentPanel",
                typeof(UIElement),
                typeof(Magnifier),
                new PropertyMetadata(default(UIElement))
            );
        public static readonly DependencyProperty MagnifiedPanelProperty
            = DependencyProperty.Register(
                "MagnifiedPanel",
                typeof(UIElement),
                typeof(Magnifier),
                new PropertyMetadata(default(UIElement))
            );
        public static readonly DependencyProperty StrokeProperty
            = DependencyProperty.Register(
                "Stroke",
                typeof(SolidColorBrush),
                typeof(Magnifier),
                new PropertyMetadata(default(SolidColorBrush))
            );

        public static readonly DependencyProperty ImageSourceProperty
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
        public UIElement ContentPanel
        {
            get => (UIElement)GetValue(ContentPanelProperty);
            set => SetValue(ContentPanelProperty, value);
        }
        public UIElement MagnifiedPanel
        {
            get => (UIElement)GetValue(MagnifiedPanelProperty);
            set => SetValue(MagnifiedPanelProperty, value);
        }
        public BitmapSource ImageBitmap
        {
            get => (BitmapSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
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
        protected Canvas MagnifierPanel { get; set; }
        public Magnifier()
        {
            Radius = 50;
            Stroke = Brushes.Black;

            MagnifierPanel = new Canvas
            {
                IsHitTestVisible = false
            };
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
            if (VisualTreeHelper.GetParent(MagnifiedPanel) is Panel container)
            {
                MagnifierImageBrush = new ImageBrush(ImageBitmap)
                {
                    ViewboxUnits = BrushMappingMode.Absolute
                };

                MagnifierRectangle = new Rectangle
                {
                    Stroke = Stroke,
                    Width = 2 * Radius,
                    Height = 2 * Radius,
                    Visibility = Visibility.Visible,
                    Fill = MagnifierImageBrush
                };

                MagnifierPanel.Children.Clear();
                MagnifierPanel.Children.Add(MagnifierRectangle);
                if (!container.Children.Contains(MagnifierPanel))
                {
                    container.Children.Add(MagnifierPanel);
                }
            }
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

        private void TriggerViewBoxUpdate()
        {
            var length = MagnifierRectangle.ActualWidth * (1 / ZoomFactor);
            var radius = length / 2;
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

            ViewBox = new Rect(x - radius, y - radius, length, length);
        }
    }
}
