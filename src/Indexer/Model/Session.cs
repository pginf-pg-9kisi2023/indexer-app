using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace Indexer.Model
{
    internal class Session
    {
        private String? _sessionPath { get; set; }
        private Config _config { get; set; }
        private List<IndexedImage> _indexedImages;
        public Session() { }

        void ExportPointsToCSV()
        {

        }

        void ExportPointsToXML()
        {

        }

        Session FromFile()
        {
            return null;
        }

        void Save()
        {

        }
    }
}
