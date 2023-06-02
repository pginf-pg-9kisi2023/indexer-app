using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
                new PropertyMetadata(default(ImageSource))
            );

        public static readonly DependencyProperty SavedPositionProperty
            = DependencyProperty.Register(
                "SavedPosition",
                typeof(String),
                typeof(Magnifier),
                new PropertyMetadata(default(String))
            );
        public static readonly DependencyProperty ImageCursorProperty
            = DependencyProperty.Register(
                "ImageCursor",
                typeof(String),
                typeof(Magnifier),
                new PropertyMetadata(default(String))
            );
        public SolidColorBrush Stroke
        {
            get => (SolidColorBrush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }
        public String SavedPosition
        {
            get => (String)GetValue(SavedPositionProperty);
            set => SetValue(SavedPositionProperty, value);
        }
        public String ImageCursor
        {
            get => (String)GetValue(ImageCursorProperty);
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

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ImageSourceProperty)
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
            if (e.Property == SavedPositionProperty || e.Property == ImageCursorProperty)
            {
                if (SavedPosition != null && ImageCursor != null)
                {
                    var length = MagnifierRectangle.ActualWidth * (1 / ZoomFactor);
                    var radius = length / 2;
                    int x = 0;
                    int y = 0;
                    if (SavedPosition != "")
                    {

                        string[] cordinates = SavedPosition.Split(',');
                        x = int.Parse(cordinates[0]);
                        y = int.Parse(cordinates[1]);

                    }
                    else if (ImageCursor != "")
                    {
                        string[] cordinates = ImageCursor.Split(',');
                        x = int.Parse(cordinates[0]);
                        y = int.Parse(cordinates[1]);
                    }

                    ViewBox = new Rect(x - radius, y - radius, length, length);
                }
            }
        }
    }
}
