using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        public SolidColorBrush Stroke
        {
            get => (SolidColorBrush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
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
            get => MagnifierBrush.Viewbox;
            set => MagnifierBrush.Viewbox = value;
        }
        protected VisualBrush MagnifierBrush { get; set; } = new();
        protected Rectangle MagnifierRectangle { get; set; } = new();
        protected Canvas MagnifierPanel { get; set; }

        public Magnifier()
        {
            ZoomFactor = 3; // 3x
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

            if (e.Property == MagnifiedPanelProperty)
            {
                if (VisualTreeHelper.GetParent(MagnifiedPanel) is Panel container)
                {
                    MagnifierBrush = new VisualBrush(ContentPanel)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute
                    };

                    MagnifierRectangle = new Rectangle
                    {
                        Stroke = Stroke,
                        Width = 2 * Radius,
                        Height = 2 * Radius,
                        Visibility = Visibility.Hidden,
                        Fill = MagnifierBrush
                    };

                    MagnifierPanel.Children.Add(MagnifierRectangle);
                    container.Children.Add(MagnifierPanel);
                    ContentPanel.MouseEnter += delegate
                    {
                        MagnifierRectangle.Visibility = Visibility.Visible;
                    };
                    ContentPanel.MouseMove += ContentPanel_OnMouseMove;
                }
            }
        }

        private void ContentPanel_OnMouseMove(object sender, MouseEventArgs e)
        {
            var length = MagnifierRectangle.ActualWidth * (1 / ZoomFactor);
            var radius = length / 2;
            Grid container = VisualTreeHelper.GetParent(ContentPanel) as Grid;
            var center = e.GetPosition(container);
            ViewBox = new Rect(center.X-radius,center.Y-radius, length, length);
           
        }
    }
}
