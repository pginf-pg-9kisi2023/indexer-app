using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Indexer.Model
{
    [DataContract(Name = "session", Namespace = "")]
    internal class Session
    {
        public string? FilePath { get; set; }
        [DataMember(Name = "config", IsRequired = true)]
        public Config Config { get; private set; }
        private List<IndexedImage> _indexedImages = new();
        public ReadOnlyCollection<IndexedImage> IndexedImages { get; private set; }
        [DataMember(Name = "images", IsRequired = true)]
        private List<IndexedImage> _xmlIndexedImages
        {
            get => _indexedImages;
            set
            {
                _indexedImages = value;
                IndexedImages = new(value);
            }
        }

        public Session(Config config)
        {
            Config = config;
            IndexedImages = new(_indexedImages);
        }

        public void ExportPointsToCSV(string filePath)
        {

        }

        public void ExportPointsToXML(string filePath)
        {

        }

        public static Session FromFile(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open);
            using var reader = XmlDictionaryReader.CreateTextReader(
                fs,
                Encoding.UTF8,
                quotas: new XmlDictionaryReaderQuotas(),
                onClose: null
            );

            var ser = new DataContractSerializer(typeof(Session));
            var session = (Session?)ser.ReadObject(reader, verifyObjectName: true);
            if (session is null)
            {
                throw new SerializationException("Failed to deserialize session file.");
            }
            session.FilePath = filePath;
            return session;
        }

        public void Save()
        {
            if (FilePath is null)
            {
                throw new InvalidOperationException("Session file path is not set.");
            }

            using var fs = new FileStream(FilePath, FileMode.Create);
            using var writer = XmlWriter.Create(
                fs,
                new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = "    ",
                    NewLineChars = "\n",
                    OmitXmlDeclaration = true
                }
            );

            var ser = new DataContractSerializer(GetType());
            ser.WriteObject(writer, this);
        }
    }
}
