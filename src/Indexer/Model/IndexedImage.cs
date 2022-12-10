using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indexer.Model
{
    internal class IndexedImage
    {
        private Image _image;
        private List<Label> _labels;
        void AddImage();
        void DeleteLabel();
        void EditLable();

        IndexedImage(Image image)
        {
            _image = image;
        }
    }
}
