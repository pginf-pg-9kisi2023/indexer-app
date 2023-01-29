using System.Runtime.Serialization;

using Indexer.Collections;
using Indexer.Collections.Generic;

namespace Indexer.Model
{
    using ReadOnlyLabelCollection = ReadOnlyKeyedCollection<string, Label>;

    [DataContract(Name = "image", Namespace = "")]
    public class IndexedImage
    {
        private Image _image;
        [DataMember(Name = "path", IsRequired = true)]
        public string ImagePath
        {
            get => _image.Path;
            private set => _image = new(value);
        }
        private LabelCollection _labels = new();
        public ReadOnlyLabelCollection Labels { get; private set; }
        [DataMember(Name = "points", IsRequired = true)]
        private LabelCollection _xmlLabels
        {
            get => _labels;
            set
            {
                _labels = value;
                Labels = new(value);
            }
        }

        public IndexedImage(Image image)
        {
            _image = image;
            Labels = new(_labels);
        }

        public void AddLabel(Label label)
        {
            _labels.Add(label);
        }

        public void DeleteLabel(string labelName)
        {
            _labels.Remove(labelName);
        }
    }
}
