using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Indexer.Collections;
using Indexer.ViewModel;

using MemoryStream = System.IO.MemoryStream;

namespace Indexer.View
{
    public class ImageWithLabels : Grid
    {
        public static readonly DependencyProperty StreamSourceProperty
            = DependencyProperty.Register(
                "StreamSource",
                typeof(MemoryStream),
                typeof(ImageWithLabels),
                new PropertyMetadata(default(MemoryStream), OnStreamSourceChange)
            );
        public MemoryStream StreamSource
        {
            get => (MemoryStream)GetValue(StreamSourceProperty);
            set => SetValue(StreamSourceProperty, value);
        }
        public static readonly DependencyProperty BitmapSourceProperty
            = DependencyProperty.Register(
                "BitmapSource",
                typeof(BitmapImage),
                typeof(ImageWithLabels),
                new PropertyMetadata(default(BitmapImage))
            );
        public BitmapImage? BitmapSource
        {
            get => (BitmapImage)GetValue(BitmapSourceProperty);
            private set => SetValue(BitmapSourceProperty, value);
        }
        public static readonly DependencyProperty CurrentLabelProperty
            = DependencyProperty.Register(
                "CurrentLabel",
                typeof(LabelViewModel),
                typeof(ImageWithLabels),
                new PropertyMetadata(default(LabelViewModel), OnCurrentLabelChange)
            );
        public LabelViewModel? CurrentLabel
        {
            get => (LabelViewModel)GetValue(CurrentLabelProperty);
            set => SetValue(CurrentLabelProperty, value);
        }
        public static readonly DependencyProperty CurrentLabelsProperty
            = DependencyProperty.Register(
                "CurrentLabels",
                typeof(LabelVMObservableCollection),
                typeof(ImageWithLabels),
                new PropertyMetadata(
                    default(LabelVMObservableCollection), OnCurrentLabelsChange
                )
            );
        public LabelVMObservableCollection CurrentLabels
        {
            get => (LabelVMObservableCollection)GetValue(CurrentLabelsProperty);
            internal set => SetValue(CurrentLabelsProperty, value);
        }
        public static readonly DependencyProperty StretchProperty =
            MemoryBackedImage.StretchProperty.AddOwner(typeof(ImageWithLabels));
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }
        public static readonly DependencyProperty StretchDirectionProperty =
            MemoryBackedImage.StretchDirectionProperty.AddOwner(
                typeof(ImageWithLabels)
            );
        public StretchDirection StretchDirection
        {
            get { return (StretchDirection)GetValue(StretchDirectionProperty); }
            set { SetValue(StretchDirectionProperty, value); }
        }

        public MemoryBackedImage Image { get; private set; } = new();
        private readonly Canvas Canvas = new();
        private readonly Dictionary<string, Canvas> LabelPoints = new();

        public ImageWithLabels()
        {
            Canvas.SetBinding(
                WidthProperty, new Binding("ActualWidth") { Source = Image }
            );
            Canvas.SetBinding(
                HeightProperty, new Binding("ActualHeight") { Source = Image }
            );
            Children.Add(Image);
            Children.Add(Canvas);
            TriggerCanvasChange();
        }

        private static void OnStreamSourceChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            var self = (ImageWithLabels)sender;
            if (self is null)
            {
                return;
            }
            self.Image.StreamSource = (MemoryStream?)e.NewValue;
            self.BitmapSource = self.Image.BitmapSource;
            self.TriggerCanvasChange();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            TriggerCanvasChange();
        }

        private void TriggerCanvasChange()
        {
            LabelPoints.Clear();
            Canvas.Children.Clear();
            if (CurrentLabels is not null)
            {
                foreach (var label in CurrentLabels)
                {
                    DrawLabel(label);
                }
            }
        }

        private static void OnCurrentLabelChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            var self = (ImageWithLabels)sender;
            if (self is null)
            {
                return;
            }
            if (e.OldValue is LabelViewModel oldValue)
            {
                self.DrawLabel(oldValue);
            }
            if (e.NewValue is LabelViewModel newValue)
            {
                self.DrawLabel(newValue);
            }
        }

        private static void OnCurrentLabelsChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            var self = (ImageWithLabels)sender;
            if (self is null)
            {
                return;
            }

            self.LabelPoints.Clear();
            self.Canvas.Children.Clear();
            if (e.OldValue is LabelVMObservableCollection oldValue)
            {
                foreach (var label in oldValue)
                {
                    label.PropertyChanged -= self.OnLabelChange;
                }
            }
            if (e.NewValue is LabelVMObservableCollection newValue)
            {
                foreach (var label in newValue)
                {
                    label.PropertyChanged += self.OnLabelChange;
                    self.DrawLabel(label);
                }
            }
        }

        private void OnLabelChange(object? sender, PropertyChangedEventArgs e)
        {
            var label = (LabelViewModel?)sender;
            if (label is null)
            {
                return;
            }
            if (e.PropertyName == nameof(LabelViewModel.Position))
            {
                DrawLabel(label);
            }
        }

        private void DrawLabel(LabelViewModel label)
        {
            if (Image.BitmapSource is null)
            {
                return;
            }
            LabelPoints.TryGetValue(label.Name, out var oldCanvas);
            if (oldCanvas is not null)
            {
                Canvas.Children.Remove(oldCanvas);
                LabelPoints.Remove(label.Name);
            }
            if (label.Position is null)
            {
                return;
            }

            var x = label.X * Image.ActualWidth / Image.BitmapSource.PixelWidth;
            var y = label.Y * Image.ActualHeight / Image.BitmapSource.PixelHeight;
            var labelCanvas = new Canvas();
            if (label == CurrentLabel)
            {
                var radius = 8;
                var verticalLine = new Line()
                {
                    X1 = x,
                    Y1 = y + radius,
                    X2 = x,
                    Y2 = y - radius,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };
                var horizontalLine = new Line()
                {
                    X1 = x - radius,
                    Y1 = y,
                    X2 = x + radius,
                    Y2 = y,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };
                labelCanvas.Children.Add(verticalLine);
                labelCanvas.Children.Add(horizontalLine);
            }
            else
            {
                var radius = 6;
                var strokeThickness = 4;
                var fillThickness = 2;
                var stroke = new Path()
                {
                    Data = new EllipseGeometry(new Point(x, y), radius, radius),
                    Stroke = Brushes.White,
                    StrokeThickness = strokeThickness,
                };
                var fill = new Path()
                {
                    Data = new EllipseGeometry(new Point(x, y), radius, radius),
                    Stroke = Brushes.Black,
                    StrokeThickness = fillThickness,
                };
                labelCanvas.Children.Add(stroke);
                labelCanvas.Children.Add(fill);
            }
            LabelPoints.Add(label.Name, labelCanvas);
            Canvas.Children.Add(labelCanvas);
        }
    }
}
