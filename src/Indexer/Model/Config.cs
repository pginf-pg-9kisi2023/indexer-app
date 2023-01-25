using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Indexer.Model
{
    [DataContract(Name = "pointsCollection", Namespace = "")]
    internal class Config
    {
        private List<Hint> _hints = new();
        public ReadOnlyCollection<Hint> Hints { get; private set; }
        [DataMember(Name = "points", IsRequired = true)]
        private List<Hint> _xmlHints
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
            Config? config = (Config?)ser.ReadObject(reader, verifyObjectName: true);
            if (config is null)
            {
                throw new SerializationException("Failed to deserialize config file.");
            }
            return config;
        }
    }
}
