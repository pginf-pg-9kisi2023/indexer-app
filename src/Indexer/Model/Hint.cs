using System.Collections.Generic;

namespace Indexer.Model
{
    internal class Hint
    {
        private string _description { get; set; }
        private string _name { get; set; }
        private readonly List<Label> _labels;
        private readonly Image _image;


        Hint(string description, string name, Image image)
        {
            _description = description;
            _name = name;
            _image = image;
            _labels = new List<Label>();
        }
    }
}
