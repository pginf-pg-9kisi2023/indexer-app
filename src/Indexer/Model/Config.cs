using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

using Indexer.Collections;
using Indexer.Collections.Generic;

namespace Indexer.Model
{
    using ReadOnlyHintCollection = ReadOnlyKeyedCollection<string, Hint>;

    [DataContract(Name = "pointsCollection", Namespace = "")]
    public class Config
    {
        private HintCollection _hints = new();
        public ReadOnlyHintCollection Hints { get; private set; }
        [DataMember(Name = "points", IsRequired = true)]
        private HintCollection _xmlHints
        {
            get => _hints;
            set
            {
                _hints = value;
                Hints = new(value);
            }
        }

        private Config()
        {
            Hints = new(_hints);
        }

        public static Config FromFile(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open);
            using var reader = XmlDictionaryReader.CreateTextReader(
                fs,
                Encoding.UTF8,
                quotas: new XmlDictionaryReaderQuotas(),
                onClose: null
            );

            var ser = new DataContractSerializer(typeof(Config));
            Config? config;
            try
            {
                config = (Config?)ser.ReadObject(reader, verifyObjectName: true);
            }
            catch (ArgumentException ex)
            {
                throw new SerializationException(ex.Message);
            }
            if (config is null)
            {
                throw new SerializationException("Failed to deserialize config file.");
            }

            var baseDirectory = Path.GetDirectoryName(filePath) ?? "";
            for (int i = 0; i < config._hints.Count; ++i)
            {
                config._hints[i] = new Hint(
                    config._hints[i].Name,
                    config._hints[i].Description,
                    Path.Combine(baseDirectory, config._hints[i].ImagePath)
                );
            }
            return config;
        }
    }
}
