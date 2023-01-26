using System;
using System.Collections.Generic;

namespace Indexer.Model
{
    internal class Session
    {
        public string? SessionFilePath { get; set; }
        public Config Config { get; private set; }
        private readonly List<IndexedImage> _indexedImages;

        public Session(Config config)
        {
            Config = config;
            _indexedImages = new();
        }

        public void ExportPointsToCSV(string filePath)
        {

        }

        public void ExportPointsToXML(string filePath)
        {

        }

        public static Session FromFile(string sessionFilePath)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {

        }
    }
}
