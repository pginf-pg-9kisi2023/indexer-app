using System;
using System.Collections.Generic;

namespace Indexer.Model
{
    internal class Config
    {
        private string _filePath { get; set; }
        private readonly List<Hint> _hints;

        Config(string filePath)
        {
            _filePath = filePath;
            _hints = new List<Hint>();
        }

        public static Config FromFile(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
