using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Indexer.Model
{
    internal class IndexedImage
    {
        private readonly Image _image;
        private readonly List<Label> _labels = new();
        public ReadOnlyCollection<Label> Labels => new(_labels);

        IndexedImage(Image image)
        {
            _image = image;
        }

        void AddImage()
        {

        }

        void DeleteLabel()
        {

        }

        void EditLabel()
        {

        }
    }
}
