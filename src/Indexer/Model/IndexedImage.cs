using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Indexer.Model
{
    [DataContract(Name = "image", Namespace = "")]
    internal class IndexedImage
    {
        private Image _image;
        [DataMember(Name = "path", IsRequired = true)]
        public string ImagePath
        {
            get => _image.Path;
            private set => _image = new(value);
        }
        private List<Label> _labels = new();
        public ReadOnlyCollection<Label> Labels { get; private set; }
        [DataMember(Name = "points", IsRequired = true)]
        private List<Label> _xmlLabels
        {
            get => _labels;
            set
            {
                _labels = value;
                Labels = new(value);
            }
        }

        internal IndexedImage(Image image)
        {
            _image = image;
            Labels = new(_labels);
        }

        public void AddLabel()
        {

        }

        public void DeleteLabel()
        {

        }

        public void EditLabel()
        {

        }
    }
}
