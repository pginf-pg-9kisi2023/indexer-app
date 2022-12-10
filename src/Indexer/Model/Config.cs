namespace Indexer.Model
{
    internal class Config
    {
        private string _filePath { get; set; };
        private List<Hint> _hints;
        Config(string filePath)
        {
            _filePath = filePath;
        }

        Config FromFile();
    }
}
