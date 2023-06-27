using System.Windows;
using System.Windows.Controls;

using Point = System.Drawing.Point;

namespace Indexer.View
{
    public class CenteringScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty PositionProperty
            = DependencyProperty.Register(
                "Position",
                typeof(Point?),
                typeof(CenteringScrollViewer),
                new PropertyMetadata(default(Point?), OnPositionChange)
            );
        public Point? Position
        {
            get => (Point?)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }
        public static readonly DependencyProperty PixelWidthProperty
            = DependencyProperty.Register(
                "PixelWidth",
                typeof(double),
                typeof(CenteringScrollViewer),
                new PropertyMetadata(default(double), OnPixelWidthChange)
            );
        public double PixelWidth
        {
            get => (double)GetValue(PixelWidthProperty);
            set => SetValue(PixelWidthProperty, value);
        }
        public static readonly DependencyProperty PixelHeightProperty
            = DependencyProperty.Register(
                "PixelHeight",
                typeof(double),
                typeof(CenteringScrollViewer),
                new PropertyMetadata(default(double), OnPixelHeightChange)
            );
        public double PixelHeight
        {
            get => (double)GetValue(PixelHeightProperty);
            set => SetValue(PixelHeightProperty, value);
        }
        public static readonly DependencyProperty IsCenteringEnabledProperty
            = DependencyProperty.Register(
                "IsCenteringEnabled",
                typeof(bool),
                typeof(CenteringScrollViewer),
                new PropertyMetadata(default(bool), OnIsCenteringEnabledChange)
            );
        public bool IsCenteringEnabled
        {
            get => (bool)GetValue(IsCenteringEnabledProperty);
            set => SetValue(IsCenteringEnabledProperty, value);
        }


        private static void OnPositionChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            UpdateScrollPosition(sender);
        }

        private static void OnPixelWidthChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            UpdateScrollPosition(sender);
        }

        private static void OnPixelHeightChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            UpdateScrollPosition(sender);
        }

        private static void OnIsCenteringEnabledChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            UpdateScrollPosition(sender);
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);
            UpdateScrollPosition();
        }

        private static void UpdateScrollPosition(DependencyObject sender)
        {
            var self = (CenteringScrollViewer)sender;
            if (self is null)
            {
                return;
            }
            self.UpdateScrollPosition();
        }

        private void UpdateScrollPosition()
        {
            if (!IsCenteringEnabled || PixelWidth == 0 || PixelHeight == 0)
            {
                return;
            }
            if (Position is Point pos)
            {
                ScrollXIntoCenter(pos.X * ExtentWidth / PixelWidth);
                ScrollYIntoCenter(pos.Y * ExtentHeight / PixelHeight);
            }
        }

        private void ScrollXIntoCenter(double x)
        {
            ScrollToHorizontalOffset(x - ViewportWidth / 2);
        }

        private void ScrollYIntoCenter(double y)
        {
            ScrollToVerticalOffset(y - ViewportHeight / 2);
        }
    }
}
