using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Indexer.Model
{
    internal class Session
    {
        public string? SessionFilePath { get; set; }
        public Config Config { get; private set; }
        private readonly List<IndexedImage> _indexedImages;
        public ReadOnlyCollection<IndexedImage> IndexedImages => new(_indexedImages);

        public Session(Config config)
        {
            Config = config;
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
