namespace Indexer.Model
{
    internal class Hint
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        private Image _image;
        public string ImagePath
        {
            get => _image.Path;
            private set => _image = new(value);
        }

        Hint(string name, string description, string imagePath)
        {
            Name = name;
            Description = description;
            _image = new Image(imagePath);
        }
    }
}
