using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Indexer.ViewModel
{
    public class ImageViewModel : ViewModelBase
    {
        private const int PropertyTagOrientation = 0x0112;

        public string Path { get; private set; }
        public MemoryStream? LoadedImage { get; set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        public int OriginalOrientation { get; private set; }

        public ImageViewModel(string path)
        {
            Path = path;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                LoadedImage = null;
            }
        }

        public void LoadImage(bool usePlaceholderIfNecessary = false)
        {
            if (LoadedImage != null)
            {
                return;
            }

            Image tmpBitmap;
            try
            {
                tmpBitmap = Image.FromFile(Path);
            }
            catch (FileNotFoundException)
            {
                using var pixel = new Bitmap(1, 1);
                pixel.SetPixel(0, 0, Color.White);
                tmpBitmap = new Bitmap(pixel, 128, 128);
            }
            using var bitmap = tmpBitmap;
            Width = bitmap.Width;
            Height = bitmap.Width;

            try
            {
                var propertyItem = bitmap.GetPropertyItem(PropertyTagOrientation);
                OriginalOrientation = propertyItem?.Value?[0] ?? 1;
            }
            catch (ArgumentException)
            {
                OriginalOrientation = 1;
            }
            // https://learn.microsoft.com/en-us/windows/win32/gdiplus/-gdiplus-constant-property-item-descriptions#propertytagorientation
            switch (OriginalOrientation)
            {
                case 2:  // 0th row: top, 0th column: right side
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
                case 3:  // 0th row: bottom, 0th column: right side
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 4:  // 0th row: bottom, 0th column: left side
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                    break;
                case 5:  // 0th row: left side, 0th column: top
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                    break;
                case 6:  // 0th row: right side, 0th column: top
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 7:  // 0th row: right side, 0th column: bottom
                    bitmap.RotateFlip(RotateFlipType.Rotate270FlipX);
                    break;
                case 8:  // 0th row: left side, 0th column: bottom
                    bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            var buffer = ms.ToArray();
            LoadedImage = new MemoryStream(
                buffer, 0, buffer.Length, writable: false, publiclyVisible: false
            );

            OnPropertyChanged(nameof(LoadedImage));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(OriginalOrientation));
        }

        // TODO: figure out what arguments would be needed here
        // and whether this is needed at all
        public void Scale()
        {
            throw new NotImplementedException();
        }

        public void UnloadImage()
        {
            LoadedImage = null;
        }
    }
}
