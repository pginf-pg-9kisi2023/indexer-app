namespace Indexer.Model
{
    internal class Image
    {
        private int _height { get; set; }
        private int _width { get; set; }
        private int _orientation { get; set; }
        private string _path { get; set; }
        private System.Drawing.Image? _loadedImage { get; set; }

        Image(int height, int width, int orientation, string path)
        {
            _height = height;
            _width = width;
            _orientation = orientation;
            _path = path;
            _loadedImage = null;
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
