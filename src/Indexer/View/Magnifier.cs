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

        private int PixelWidth => (int)ActualWidth - 2;
        private int PixelHeight => (int)ActualHeight - 2;
        private double FillThickness => ZoomFactor + 2;
        private double StrokeThickness => FillThickness + 2;
        private double CrosshairOffset =>
            Math.Ceiling(StrokeThickness / 2) + ZoomFactor;
        private System.Drawing.Image? BaseImage;
        private System.Drawing.Graphics? Graphics;
        private System.Drawing.Bitmap? Bitmap;
        private readonly MemoryStream ResultStream = new();
        private readonly Image MagnifierImage =
            new()
            {
                Stretch = Stretch.None,
                SnapsToDevicePixels = true,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
        private readonly Canvas MagnifierCanvas = new() { SnapsToDevicePixels = true };

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

            UpdateCrosshair();
            UpdateMagnifierImage(resetViewBox: true);
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
            self.BaseImage?.Dispose();
            if (self.StreamSource is null)
            {
                self.BaseImage = null;
            }
            else
            {
                self.BaseImage = System.Drawing.Image.FromStream(self.StreamSource);
            }
            self.CreateGraphicsObjects();
            self.UpdateMagnifierImage(resetViewBox: true);
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
            self.UpdateCrosshair();
            self.UpdateMagnifierImage(resetViewBox: true);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            CreateGraphicsObjects();
            UpdateCrosshair();
            UpdateMagnifierImage(resetViewBox: true);
        }

        private void CreateGraphicsObjects()
        {
            Graphics?.Dispose();
            Bitmap?.Dispose();
            if (BaseImage != null)
            {
                Bitmap = new System.Drawing.Bitmap(
                    PixelWidth,
                    PixelHeight,
                    BaseImage.PixelFormat
                );
                Graphics = System.Drawing.Graphics.FromImage(Bitmap);
                // With integer scaling, this will ensure that each pixel
                // becomes factor x factor square of original pixel's color.
                Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                // This is needed so that the first pixel is not partially offset
                // into negative coordinates.
                Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            }
            else
            {
                Bitmap = null;
                Graphics = null;
            }
        }

        private void UpdateCrosshair()
        {
            MagnifierCanvas.Children.Clear();
            CreateVerticalLines();
            CreateHorizontalLines();
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

            self.UpdateMagnifierImage();
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

            self.UpdateMagnifierImage();
        }

        private void OnLabelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y")
            {
                UpdateMagnifierImage();
            }
        }

        private void UpdateMagnifierImage(bool resetViewBox = false)
        {
            if (BaseImage == null)
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
            // depending on which property change is triggered first,
            // we might end up with a point for a different image
            // that could potentially by out of bounds
            x = Math.Min(x, BaseImage.Width - 1);
            y = Math.Min(y, BaseImage.Height - 1);
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
            var rectWidth = Math.Min(PixelWidth, BaseImage.Width * ZoomFactor - rectX);
            var rectHeight = Math.Min(PixelHeight, BaseImage.Height * ZoomFactor - rectY);

            var sourceRect = new Int32Rect(rectX, rectY, rectWidth, rectHeight);
            var imageBitmap = GenerateImageBitmap(sourceRect);
            var transform = new TranslateTransform(offsetX, offsetY);

            MagnifierImage.Source = imageBitmap;
            MagnifierImage.RenderTransform = transform;
            MagnifierImage.Clip = new RectangleGeometry(new Rect(0, 0, rectWidth, rectHeight));
        }

        private BitmapImage? GenerateImageBitmap(Int32Rect sourceRect)
        {
            if (BaseImage is null)
            {
                return null;
            }
            if (!sourceRect.HasArea)
            {
                return null;
            }

            // Top-left corner of the source portion of the image.
            // If sourceRect's position is mid-pixel, we round down
            // and put the offset in the destX/Y.
            var startX = sourceRect.X / ZoomFactor;
            var startY = sourceRect.Y / ZoomFactor;
            var destX = -(sourceRect.X % ZoomFactor);
            var destY = -(sourceRect.Y % ZoomFactor);
            // Bottom-right corner of the source portion of the image.
            // If this is mid-pixel, we round up.
            var stopX = (sourceRect.X + sourceRect.Width - 1) / ZoomFactor + 1;
            var stopY = (sourceRect.Y + sourceRect.Height - 1) / ZoomFactor + 1;
            // The actual width of the source portion of the image.
            var width = stopX - startX;
            var height = stopY - startY;

            Graphics!.DrawImage(
                BaseImage,
                new System.Drawing.Rectangle(
                    destX, destY, width * ZoomFactor, height * ZoomFactor
                ),
                startX, startY, width, height,
                System.Drawing.GraphicsUnit.Pixel
            );
            ResultStream.SetLength(0);
            Bitmap!.Save(ResultStream, ImageFormat.Bmp);
            ResultStream.Seek(0, SeekOrigin.Begin);

            {
                var bitmapSource = new BitmapImage();
                bitmapSource.BeginInit();
                bitmapSource.StreamSource = ResultStream;
                bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
                bitmapSource.EndInit();
                return bitmapSource;
            }
        }
    }
}
