namespace Indexer.Model
{
    internal class Image
    {
        private const int PropertyTagOrientation = 0x0112;

        public string Path { get; private set; }
        private System.Drawing.Image? _loadedImage { get; set; }
        public int Height => _loadedImage is null ? 0 : _loadedImage.Height;
        public int Width => _loadedImage is null ? 0 : _loadedImage.Width;
        public int Orientation
        {
            get
            {
                if (_loadedImage is null)
                {
                    return 0;
                }
                var propertyItem = _loadedImage.GetPropertyItem(PropertyTagOrientation);
                if (propertyItem is null || propertyItem.Value is null)
                {
                    return 0;
                }
                return propertyItem.Value[0];
            }
        }

        internal Image(string path)
        {
            Path = path;
        }

        void LoadImage()
        {

        }

        void Scale()
        {

        }

        void UnloadImage()
        {

        }
    }
}
