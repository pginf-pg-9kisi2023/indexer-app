using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Indexer.Model
{
    internal class Session
    {
        private String? _sessionPath { get; set; }
        private Config _config { get; set; }
        private readonly List<IndexedImage> _indexedImages;
        public Session(String? sessionPath, Config conifg, List<IndexedImage> indexedImages)
        {
            _sessionPath = sessionPath;
            _config = conifg;
            _indexedImages = indexedImages;
            InitSessionFile();
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
        public void InitSessionFile()
        {
            string fileName = _sessionPath ?? Path.GetTempFileName();
            FileStream fileStream = File.Create(fileName);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Session));
            XmlWriter xmlWriter = new XmlTextWriter(fileStream, Encoding.Unicode);
            xmlSerializer.Serialize(xmlWriter, this);
            xmlWriter.Close();
        }
    }
}
