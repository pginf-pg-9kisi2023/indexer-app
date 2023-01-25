using System;
using System.Collections.Generic;

namespace Indexer.Model
{
    internal class Session
    {
        private String? _sessionPath { get; set; }
        private Config _config { get; set; }
        private readonly List<IndexedImage> _indexedImages;

        public Session(String? sessionPath, Config config, List<IndexedImage> indexedImages)
        {
            _sessionPath = sessionPath;
            _config = config;
            _indexedImages = indexedImages;
        }

        void ExportPointsToCSV()
        {

        }

        void ExportPointsToXML()
        {

        }

        Session? FromFile()
        {
            return null;
        }

        void Save()
        {

        }
    }
}
