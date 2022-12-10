using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indexer.Model
{
    internal class Label
    {
        private int _x { get; set; };
        private int _y { get; set; };
        private string _name { get; set; };

    Label(int x, int y,string name)
        {
            _x = x;
            _y = y;
            _name = name;
        }
    }
}
