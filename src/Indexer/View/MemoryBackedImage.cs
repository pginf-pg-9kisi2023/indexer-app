using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Indexer.View
{
    public class MemoryBackedImage : Image
    {
        public static readonly DependencyProperty StreamSourceProperty
            = DependencyProperty.Register(
                "StreamSource",
                typeof(MemoryStream),
                typeof(MemoryBackedImage),
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
                typeof(MemoryBackedImage),
                new PropertyMetadata(default(BitmapImage))
            );
        public BitmapImage? BitmapSource
        {
            get => (BitmapImage)GetValue(BitmapSourceProperty);
            private set => SetValue(BitmapSourceProperty, value);
        }

        public MemoryBackedImage()
        {
            UpdateSource();
        }

        private static void OnStreamSourceChange(
            DependencyObject sender, DependencyPropertyChangedEventArgs e
        )
        {
            var self = (MemoryBackedImage)sender;
            if (self is null)
            {
                return;
            }
            self.UpdateSource();
        }

        private void UpdateSource()
        {
            if (StreamSource is null)
            {
                BitmapSource = null;
                Source = BitmapSource;
                return;
            }
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = StreamSource;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            BitmapSource = bitmapImage;
            Source = BitmapSource;
        }
    }
}
