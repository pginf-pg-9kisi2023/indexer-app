using System;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Point = System.Drawing.Point;

namespace Indexer.View
{
    public class Magnifier : Border
    {
        public static readonly DependencyProperty StreamSourceProperty
            = DependencyProperty.Register(
                "StreamSource",
                typeof(MemoryStream),
                typeof(Magnifier),
                new PropertyMetadata(default(MemoryStream), OnStreamSourceChange)
            );
        public MemoryStream StreamSource
        {
            get => (MemoryStream)GetValue(StreamSourceProperty);
            set => SetValue(StreamSourceProperty, value);
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
                typeof(int),
                typeof(Magnifier),
                new PropertyMetadata(default(int), OnZoomFactorChange)
            );
        public int ZoomFactor
        {
            get => (int)GetValue(ZoomFactorProperty);
            set => SetValue(ZoomFactorProperty, value);
        }

        protected int PixelWidth => (int)ActualWidth - 2;
        protected int PixelHeight => (int)ActualHeight - 2;
        protected double FillThickness => ZoomFactor + 2;
        protected double StrokeThickness => FillThickness + 2;
        protected double CrosshairOffset =>
            Math.Ceiling(StrokeThickness / 2) + ZoomFactor;
        protected BitmapSource? ImageBitmap { get; set; }
        protected Image MagnifierImage { get; set; } =
            new()
            {
                Stretch = Stretch.None,
                SnapsToDevicePixels = true,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
        protected Canvas MagnifierCanvas { get; set; } =
            new() { SnapsToDevicePixels = true };
        protected Rectangle MagnifierRectangle { get; set; } = new();

        public Magnifier()
        {
            RenderOptions.SetBitmapScalingMode(
                MagnifierImage, BitmapScalingMode.NearestNeighbor
            );
            MagnifierImage.SetBinding(
                Image.WidthProperty,
                new Binding("Source.PixelWidth")
                {
                    RelativeSource = RelativeSource.Self
                }
            );
            MagnifierImage.SetBinding(
                Image.HeightProperty,
                new Binding("Source.PixelHeight")
                {
                    RelativeSource = RelativeSource.Self
                }
            );

            var grid = new Grid() { ClipToBounds = true };
            grid.Children.Add(MagnifierImage);
            grid.Children.Add(MagnifierCanvas);

            SnapsToDevicePixels = true;
            BorderThickness = new(1);
            BorderBrush = Stroke;
            Child = grid;

            TriggerMagnifierUpdate();
        }

        private static void OnStreamSourceChange(
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

        private static void OnZoomFactorChange(
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

        private void UpdateImageBitmap()
        {
            if (StreamSource is null)
            {
                ImageBitmap = null;
                return;
            }

            var regular = System.Drawing.Image.FromStream(StreamSource);
            using var result = new System.Drawing.Bitmap(
                regular.Width * ZoomFactor,
                regular.Height * ZoomFactor,
                regular.PixelFormat
            );
            using (var g = System.Drawing.Graphics.FromImage(result))
            {
                // With integer scaling, this will ensure that each pixel
                // becomes factor x factor square of original pixel's color.
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                // This is needed so that the first pixel is not partially offset
                // into negative coordinates.
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(regular, 0, 0, result.Width, result.Height);
            }
            var ms = new MemoryStream();
            result.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            {
                var bitmapSource = new BitmapImage();
                bitmapSource.BeginInit();
                bitmapSource.StreamSource = ms;
                bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
                bitmapSource.EndInit();
                ImageBitmap = bitmapSource;
            }
        }

        private void TriggerMagnifierUpdate()
        {
            UpdateImageBitmap();
            TriggerMagnifierRectangleUpdate();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            TriggerMagnifierRectangleUpdate();
        }

        private void TriggerMagnifierRectangleUpdate()
        {
            MagnifierCanvas.Children.Clear();
            CreateVerticalLines();
            CreateHorizontalLines();

            TriggerViewBoxUpdate(resetViewBox: true);
        }

        private void CreateVerticalLines()
        {
            // top line
            CreateLine(
                X1: PixelWidth / 2,
                Y1: PixelHeight / 2 - CrosshairOffset + (ZoomFactor % 2),
                X2: PixelWidth / 2,
                Y2: 0
            );
            // bottom line
            CreateLine(
                X1: PixelWidth / 2,
                Y1: PixelHeight / 2 + CrosshairOffset,
                X2: PixelWidth / 2,
                Y2: PixelHeight
            );
        }

        private void CreateHorizontalLines()
        {
            // left line
            CreateLine(
                X1: PixelWidth / 2 - CrosshairOffset + (ZoomFactor % 2),
                Y1: PixelHeight / 2 + (ZoomFactor % 2),
                X2: 0,
                Y2: PixelHeight / 2 + (ZoomFactor % 2)
            );
            // right line
            CreateLine(
                X1: PixelWidth / 2 + CrosshairOffset,
                Y1: PixelHeight / 2 + (ZoomFactor % 2),
                X2: PixelWidth,
                Y2: PixelHeight / 2 + (ZoomFactor % 2)
            );
        }

        private void CreateLine(double X1, double Y1, double X2, double Y2)
        {
            // These are relative to stroke thickness, as required by StrokeDashArray.
            var dashSize = 2;
            var gapSize = 2;

            var stroke = new Line { X1 = X1, Y1 = Y1, X2 = X2, Y2 = Y2 };
            stroke.Stroke = Brushes.Black;
            stroke.StrokeThickness = StrokeThickness;
            stroke.StrokeDashArray = new DoubleCollection(
                new double[] { dashSize, gapSize }
            );
            MagnifierCanvas.Children.Add(stroke);

            // StrokeDashArray is relative to StrokeThickness
            // so we need to calculate the ratio to allow common point of reference.
            var ratio = StrokeThickness / FillThickness;
            // Difference between stroke and fill thickness, allows us to
            // calculate the offsets for StrokeDashOffset, dash, and gap size.
            var diff = StrokeThickness - FillThickness;

            var fill = new Line { X1 = X1, Y1 = Y1, X2 = X2, Y2 = Y2 };
            fill.Stroke = Brushes.White;
            fill.StrokeThickness = FillThickness;
            // Offset by half of the diff to keep the fill in center.
            // Divided by FillThickness since `diff` is an absolute value.
            fill.StrokeDashOffset = -diff / (2 * FillThickness);
            fill.StrokeDashArray = new DoubleCollection(
                new double[]
                {
                    // fill dash is smaller than stroke by `diff` (absolute value)
                    ratio * (dashSize - diff / StrokeThickness),
                    // fill gap is larger than stroke by `diff` (absolute value)
                    ratio * (gapSize + diff / StrokeThickness),
                }
            );
            MagnifierCanvas.Children.Add(fill);
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

        private void TriggerViewBoxUpdate(bool resetViewBox = false)
        {
            if (ImageBitmap == null)
            {
                MagnifierImage.Source = null;
                return;
            }

            // The first pixel of the scaled pixel ("pixel square").
            // These are simply coords multiplied by the zoom factor.
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
            x *= ZoomFactor;
            y *= ZoomFactor;

            // The position within the scaled image of the top-left corner
            // of the area the bitmap will be cropped to.
            // Since we want the center of the magnifier to be the center of the scaled
            // pixel, we have to increment this by half of the zoom factor as well.
            var rectX = x - PixelWidth / 2 + ZoomFactor / 2;
            var rectY = y - PixelHeight / 2 + ZoomFactor / 2;

            // If a calculated coordinate of the top-left corner is negative,
            // we have to set it to 0, making sure to store this offset in order to
            // later use it as image's TranslateTransform.
            var offsetX = rectX < 0 ? -rectX : 0;
            var offsetY = rectY < 0 ? -rectY : 0;
            rectX = Math.Max(rectX, 0);
            rectY = Math.Max(rectY, 0);

            // The actual size of the area the bitmap will be cropped to.
            // This is just the width of magnifier unless the distance to
            // the bottom-right corner of the scaled image is less than that
            // in which case, it's the size of whatever the remaining area is.
            var rectWidth = Math.Min(PixelWidth, ImageBitmap.PixelWidth - rectX);
            var rectHeight = Math.Min(PixelHeight, ImageBitmap.PixelHeight - rectY);

            var sourceRect = new Int32Rect(rectX, rectY, rectWidth, rectHeight);
            BitmapSource? cropped = null;
            if (sourceRect.HasArea)
            {
                cropped = new CroppedBitmap(ImageBitmap, sourceRect);
            }
            var transform = new TranslateTransform(offsetX, offsetY);

            MagnifierImage.Source = cropped;
            MagnifierImage.RenderTransform = transform;
        }
    }
}
