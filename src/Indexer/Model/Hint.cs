using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indexer.Model
{
    internal class Hint
    {
        private string _description { get; set; };
        private string _name { get; set; };
        private List<Label> _labels;
        private Image _image;


        Hint(string description, string name,Image image)
        {
            _description = description;
            _name = name;
            _image = image;
        }
    }
}
