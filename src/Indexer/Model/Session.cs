using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
        public Hint? CurrentHint
        {
            get
            {
                if (CurrentImage == null)
                {
                    return null;
                }
                if (CurrentImage.CurrentHintName == null)
                {
                    return Config.Hints.First();
                }
                return Config.Hints[CurrentImage.CurrentHintName];
            }
            set
            {
                if (CurrentImage == null)
                {
                    throw new ArgumentException(
                        "This can only be set if CurrentImage is set."
                    );
                }
                if (value == Config.Hints.First())
                {
                    CurrentImage.CurrentHintName = null;
                }
                else
                {
                    CurrentImage.CurrentHintName = value?.Name;
                }
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
            using var fs = new FileStream(filePath, FileMode.Create);
            using var sw = new StreamWriter(fs);
            sw.NewLine = "\n";
            var header = new StringBuilder();
            var data = new StringBuilder();
            header.Append("\"image_filename\"");
            char delimiter = ',';
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            foreach (var hint in Config.Hints)
            {
                header.AppendFormat(cultureInfo, "{0}\"{1} x\"{2}\"{3} y\"", delimiter, hint.Name, delimiter, hint.Name);
            }
            sw.WriteLine(header.ToString());
            foreach (var image in _indexedImages)
            {
                data.AppendFormat(cultureInfo, "\"{0}\"", image.ImagePath);
                foreach (var hint in Config.Hints)
                {
                    if (image.Labels.TryGetValue(hint.Name, out Label? toExport))
                    {
                        if (toExport != null)
                        {
                            data.AppendFormat(cultureInfo, "{0}{1}{2}{3}", delimiter, toExport.X, delimiter, toExport.Y);
                        }
                    }
                    else
                    {
                        data.AppendFormat(cultureInfo, "{0}{1}", delimiter, delimiter);
                    }
                }
                sw.WriteLine(data.ToString());
                data.Clear();
            }
        }

        public void ExportPointsToXML(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Create);
            using var sw = new StreamWriter(fs);
            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true,
                IndentChars = "    ",
                NewLineChars = "\n"
            };
            using var xmlWriter = XmlWriter.Create(sw, xmlWriterSettings);
            xmlWriter.WriteStartElement("images");
            foreach (var image in _indexedImages)
            {
                xmlWriter.WriteStartElement("image");
                xmlWriter.WriteAttributeString("path", image.ImagePath);
                xmlWriter.WriteStartElement("points");
                foreach (var label in image.Labels)
                {
                    xmlWriter.WriteStartElement("point");
                    xmlWriter.WriteAttributeString("name", label.Name);
                    xmlWriter.WriteStartElement("x");
                    xmlWriter.WriteValue(label.X);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("y");
                    xmlWriter.WriteValue(label.Y);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
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

        public void AnalyzeImages(string filePath)
        {
            using (var p = new Process())
            {
                p.StartInfo = new()
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    FileName = filePath,
                };
                p.OutputDataReceived += HandleAnalyzedImageOutput;
                p.Start();
                p.BeginOutputReadLine();
                using (var writer = p.StandardInput)
                {
                    var line = new StringBuilder();
                    foreach (var img in _indexedImages)
                    {
                        line.Append(img.ImagePath);
                        foreach (var hint in Config.Hints)
                        {
                            if (!img.Labels.Contains(hint.Name))
                            {
                                line.AppendFormat(
                                    CultureInfo.InvariantCulture, "\0{0}", hint.Name
                                );
                            }
                        }
                        writer.WriteLine(line.ToString());
                        line.Clear();
                    }
                }
                p.WaitForExit();
            }
        }

        private void HandleAnalyzedImageOutput(object sender, DataReceivedEventArgs e)
        {
            var line = e.Data;
            if (line is null)
            {
                return;
            }

            var data = line.Split("\0");
            var imgPath = data[0];
            if (!_indexedImages.TryGetValue(imgPath, out var image))
            {
                return;
            }
            var labelName = data[1];
            var x = int.Parse(data[2], CultureInfo.InvariantCulture);
            var y = int.Parse(data[3], CultureInfo.InvariantCulture);

            image.AddLabel(new Label(labelName, x, y));
        }
    }
}
