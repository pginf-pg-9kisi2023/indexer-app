using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Point = System.Drawing.Point;

namespace Indexer.View
{
    public class Magnifier : DockPanel
    {
        public static readonly DependencyProperty ImageBitmapProperty
            = DependencyProperty.Register(
                "ImageBitmap",
                typeof(ImageSource),
                typeof(Magnifier),
                new PropertyMetadata(default(ImageSource), OnImageBitmapChange)
            );
        public BitmapSource ImageBitmap
        {
            get => (BitmapSource)GetValue(ImageBitmapProperty);
            set => SetValue(ImageBitmapProperty, value);
        }

        public static readonly DependencyProperty SavedPositionProperty
            = DependencyProperty.Register(
                "SavedPosition",
                typeof(Point?),
                typeof(Magnifier),
                new PropertyMetadata(default(Point?), OnSavedPositionChange)
            );
        public Point? SavedPosition
        {
            get => (Point?)GetValue(SavedPositionProperty);
            set => SetValue(SavedPositionProperty, value);
        }
        public static readonly DependencyProperty ImageCursorProperty
            = DependencyProperty.Register(
                "ImageCursor",
                typeof(Point?),
                typeof(Magnifier),
                new PropertyMetadata(default(Point?), OnImageCursorChange)
            );
        public Point? ImageCursor
        {
            get => (Point?)GetValue(ImageCursorProperty);
            set => SetValue(ImageCursorProperty, value);
        }

        public static readonly DependencyProperty StrokeProperty
            = DependencyProperty.Register(
                "Stroke",
                typeof(SolidColorBrush),
                typeof(Magnifier),
                new PropertyMetadata(Brushes.Black)
            );
        public SolidColorBrush Stroke
        {
            get => (SolidColorBrush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }
        public static readonly DependencyProperty ZoomFactorProperty
            = DependencyProperty.Register(
                "ZoomFactor",
                typeof(double),
                typeof(Magnifier),
                new PropertyMetadata(default(double), OnZoomFactorChange)
            );
        public double ZoomFactor
        {
            get => (double)GetValue(ZoomFactorProperty);
            set => SetValue(ZoomFactorProperty, value);
        }

        protected double FillThickness => ZoomFactor + 1;
        protected double StrokeThickness => 2 * FillThickness;
        protected double CrosshairOffset => 1.5 * FillThickness;
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
            TriggerMagnifierRectangleUpdate();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            TriggerMagnifierRectangleUpdate();
        }

        private void TriggerMagnifierRectangleUpdate()
        {
            MagnifierRectangle = new Rectangle
            {
                Stroke = Stroke,
                Width = ActualWidth,
                Height = ActualHeight,
                Visibility = Visibility.Visible,
                Fill = MagnifierImageBrush
            };

            Canvas canvas = new Canvas();
            canvas.Children.Add(MagnifierRectangle);
            if (ActualWidth - 2 * MagnifierRectangle.StrokeThickness >= StrokeThickness)
            {
                CreateVerticalLines(canvas);
                CreateHorizontalLines(canvas);
            }

            Children.Clear();
            Children.Add(canvas);
            TriggerViewBoxUpdate(resetViewBox: true);
        }

        private void CreateVerticalLines(Canvas canvas)
        {
            // top line
            CreateLine(
                canvas,
                X1: ActualWidth / 2,
                Y1: ActualHeight / 2 - CrosshairOffset,
                X2: ActualWidth / 2,
                Y2: 0
            );
            // bottom line
            CreateLine(
                canvas,
                X1: ActualWidth / 2,
                Y1: ActualHeight / 2 + CrosshairOffset,
                X2: ActualWidth / 2,
                Y2: ActualHeight
            );
        }

        private void CreateHorizontalLines(Canvas canvas)
        {
            // left line
            CreateLine(
                canvas,
                X1: ActualWidth / 2 - CrosshairOffset,
                Y1: ActualHeight / 2,
                X2: 0,
                Y2: ActualHeight / 2
            );
            // right line
            CreateLine(
                canvas,
                X1: ActualWidth / 2 + CrosshairOffset,
                Y1: ActualHeight / 2,
                X2: ActualWidth,
                Y2: ActualHeight / 2
            );
        }

        private void CreateLine(
            Canvas canvas, double X1, double Y1, double X2, double Y2
        )
        {
            var stroke = new Line { X1 = X1, Y1 = Y1, X2 = X2, Y2 = Y2 };
            stroke.Stroke = Brushes.Black;
            stroke.StrokeThickness = StrokeThickness;
            stroke.StrokeDashArray = new DoubleCollection(new double[] { 2, 1 });
            canvas.Children.Add(stroke);

            var fill = new Line { X1 = X1, Y1 = Y1, X2 = X2, Y2 = Y2 };
            fill.Stroke = Brushes.White;
            fill.StrokeThickness = FillThickness;
            fill.StrokeDashOffset = -0.5;
            fill.StrokeDashArray = new DoubleCollection(new double[] { 3, 3 });
            canvas.Children.Add(fill);
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

        private static void OnSavedPositionChange(
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

        private void OnLabelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y")
            {
                TriggerViewBoxUpdate();
            }
        }

        private static void OnZoomFactorChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            var self = (Magnifier)sender;
            if (self is null)
            {
                return;
            }
            self.TriggerMagnifierRectangleUpdate();
        }

        private void TriggerViewBoxUpdate(bool resetViewBox = false)
        {
            if (ImageBitmap == null)
            {
                ViewBox = new Rect(0, 0, 0, 0);
                return;
            }
            int x = 0;
            int y = 0;
            if (SavedPosition is Point savedPosition)
            {
                x = savedPosition.X;
                y = savedPosition.Y;
            }
            else if (ImageCursor is Point cursor)
            {
                x = cursor.X;
                y = cursor.Y;
            }
            else if (!resetViewBox)
            {
                // keep the current viewbox
                return;
            }
            var factorX = 96 / ImageBitmap.DpiX;
            var factorY = 96 / ImageBitmap.DpiY;
            var width = factorX * ActualWidth / ZoomFactor;
            var height = factorY * ActualHeight / ZoomFactor;
            ViewBox = new Rect(
                factorX * (x + 0.5) - width / 2,
                factorY * (y + 0.5) - height / 2,
                width,
                height
            );
        }
    }
}
