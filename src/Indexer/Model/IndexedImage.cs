using System.Runtime.Serialization;

using Indexer.Collections;
using Indexer.Collections.Generic;

namespace Indexer.Model
{
    using ReadOnlyLabelCollection = ReadOnlyKeyedCollection<string, Label>;

    [DataContract(Name = "image", Namespace = "")]
    public class IndexedImage
    {
        [DataMember(Name = "path", IsRequired = true)]
        public string ImagePath { get; private set; }
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
        // This is `null` if labels for all hints are set and switching to this image
        // should use the first hint as the current hint.
        [DataMember(Name = "currentHintName", EmitDefaultValue = false)]
        public string? CurrentHintName { get; set; }

        public IndexedImage(string imagePath)
        {
            ImagePath = imagePath;
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
