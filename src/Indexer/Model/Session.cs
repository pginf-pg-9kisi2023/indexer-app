using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

using Indexer.Collections;
using Indexer.Collections.Generic;

namespace Indexer.Model
{
    using ReadOnlyIndexedImageCollection = ReadOnlyKeyedCollection<string, IndexedImage>;

    [DataContract(Name = "session", Namespace = "")]
    public class Session
    {
        public string? FilePath { get; set; }
        [DataMember(Name = "config", IsRequired = true)]
        public Config Config { get; private set; }
        private IndexedImageCollection _indexedImages = new();
        public ReadOnlyIndexedImageCollection IndexedImages { get; private set; }
        [DataMember(Name = "images", IsRequired = true)]
        private IndexedImageCollection _xmlIndexedImages
        {
            get => _indexedImages;
            set
            {
                _indexedImages = value;
                IndexedImages = new(value);
            }
        }
        [DataMember(Name = "currentImageIndex", IsRequired = true)]
        public int? CurrentImageIndex { get; set; }
        public IndexedImage? CurrentImage
        {
            get
            {
                var collection = (Collection<IndexedImage>)_indexedImages;
                if (CurrentImageIndex is int idx)
                {
                    return collection[idx];
                }
                return null;
            }
        }

        public Session(Config config)
        {
            Config = config;
            IndexedImages = new(_indexedImages);
        }

        public void AddIndexedImage(IndexedImage indexedImage)
        {
            _indexedImages.Add(indexedImage);
            if (CurrentImageIndex is null)
            {
                CurrentImageIndex = 0;
            }
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
            Session? session;
            try
            {
                session = (Session?)ser.ReadObject(reader, verifyObjectName: true);
            }
            catch (ArgumentException ex)
            {
                throw new SerializationException(ex.Message);
            }
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
