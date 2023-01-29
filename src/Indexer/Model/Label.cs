using System.Runtime.Serialization;

namespace Indexer.Model
{
    [DataContract(Name = "point", Namespace = "")]
    public class Label
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; private set; }
        [DataMember(Name = "x", IsRequired = true)]
        public int X { get; set; }
        [DataMember(Name = "y", IsRequired = true)]
        public int Y { get; set; }

        public Label(string name, int x = 0, int y = 0)
        {
            Name = name;
            X = x;
            Y = y;
        }
    }
}
