using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Indexer.Model
{
    internal class Config
    {
        private readonly List<Hint> _hints;
        public ReadOnlyCollection<Hint> Hints => new(_hints);

        private Config()
        {
            _hints = new List<Hint>();
        }

        public static Config FromFile(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
