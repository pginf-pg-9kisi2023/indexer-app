using System.Runtime.Serialization;

namespace Indexer.Model
{
    [DataContract(Name = "point", Namespace = "")]
    public class Hint
    {
        [DataMember(Name = "name", Order = 0, IsRequired = true)]
        public string Name { get; private set; }
        [DataMember(Name = "description", Order = 1, IsRequired = true)]
        public string Description { get; private set; }
        [DataMember(Name = "imageRef", Order = 2, IsRequired = true)]
        public string ImagePath { get; private set; }

        public Hint(string name, string description, string imagePath)
        {
            Name = name;
            Description = description;
            ImagePath = imagePath;
        }
    }
}
