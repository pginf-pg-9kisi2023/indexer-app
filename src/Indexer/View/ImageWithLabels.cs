using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Indexer.Collections;
using Indexer.ViewModel;

namespace Indexer.View
{
    public class ImageWithLabels : MemoryBackedImage
    {
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
        private DrawingVisual? Drawing;

        public ImageWithLabels() { }

        private static void OnCurrentLabelChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            var self = (ImageWithLabels)sender;
            if (self is null)
            {
                return;
            }
            self.DrawLabels();
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
            if (e.OldValue is LabelVMObservableCollection oldValue)
            {
                oldValue.CollectionChanged -= self.OnLabelsCollectionChanged;
            }
            if (e.NewValue is LabelVMObservableCollection newValue)
            {
                newValue.CollectionChanged += self.OnLabelsCollectionChanged;
            }
            self.DrawLabels();
        }

        private void OnLabelsCollectionChanged(
            object? sender, NotifyCollectionChangedEventArgs e
        )
        {
            if (Drawing != null && e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems is IList<LabelViewModel> labels)
                {
                    var drawingContext = Drawing.RenderOpen();
                    foreach (var label in labels)
                    {
                        DrawLabel(drawingContext, label);
                    }
                    drawingContext.Close();
                    UpdateSource();
                }
                return;
            }
            DrawLabels();
        }

        private void DrawLabels()
        {
            if (BitmapSource is null)
            {
                return;
            }
            Drawing = new DrawingVisual();
            var drawingContext = Drawing.RenderOpen();
            drawingContext.DrawImage(
                BitmapSource,
                new Rect(0, 0, BitmapSource.PixelWidth, BitmapSource.PixelHeight)
            );
            foreach (var label in CurrentLabels)
            {
                DrawLabel(drawingContext, label);
            }
            drawingContext.Close();
            UpdateSource();
        }

        private void UpdateSource()
        {
            if (BitmapSource is null)
            {
                return;
            }
            var rtb = new RenderTargetBitmap(
                BitmapSource.PixelWidth,
                BitmapSource.PixelHeight,
                96,
                96,
                PixelFormats.Pbgra32
            );
            rtb.Render(Drawing);
            Source = rtb;
        }

        private void DrawLabel(
            [NotNull] DrawingContext drawingContext, LabelViewModel label
        )
        {
            if (label == CurrentLabel)
            {
                Pen pen = new Pen(Brushes.Red, 2);
                drawingContext.DrawLine(
                    pen,
                    new Point(label.X, label.Y + 10),
                    new Point(label.X, label.Y - 10)
                );
                drawingContext.DrawLine(
                    pen,
                    new Point(label.X + 10, label.Y),
                    new Point(label.X - 10, label.Y)
                );
            }
            else
            {
                drawingContext.DrawEllipse(
                    null, new Pen(Brushes.Black, 2), new Point(label.X, label.Y), 10, 10
                );
                drawingContext.DrawEllipse(
                    null, new Pen(Brushes.White, 2), new Point(label.X, label.Y), 8, 8
                );
            }
        }
    }
}
