using System.Collections.Generic;

namespace Indexer.Model
{
    internal class IndexedImage
    {
        private readonly Image _image;
        private readonly List<Label> _labels;
        IndexedImage(Image image)
        {
            _image = image;
            _labels = new List<Label>();
        }
        void AddImage()
        {

        }
        void DeleteLabel()
        {

        }
        void EditLable()
        {

        }


    }
}
