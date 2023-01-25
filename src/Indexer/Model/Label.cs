using System.Runtime.Serialization;

namespace Indexer.Model
{
    [DataContract(Name = "point", Namespace = "")]
    internal class Label
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; private set; }
        [DataMember(Name = "x", IsRequired = true)]
        public int X { get; private set; }
        [DataMember(Name = "y", IsRequired = true)]
        public int Y { get; private set; }

        internal Label(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
        }
    }
}
