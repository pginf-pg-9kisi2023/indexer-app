using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Indexer.Collections;

namespace Indexer.ViewModel
{
    public class ImageSelectionViewModel : ViewModelBase
    {
        public ReadOnlyCollection<string> ImageFileExtensions { get; private set; }
            = new List<String> { ".jpg", ".jpeg", ".png" }.AsReadOnly();

        public StringObservableKeyedCollection Files { get; private set; }
        public StringObservableKeyedCollection SelectedFiles { get; private set; }

        public ImageSelectionViewModel()
        {
            Files = new();
            SelectedFiles = new();
        }

        public void AddFiles(IEnumerable<string> files)
        {
            Files.AddRange(files);
        }

        public void AddFolder(string folder)
        {
            var di = new DirectoryInfo(folder);
            Files.AddRange(
                from fi in di.EnumerateFiles("*.*", SearchOption.AllDirectories)
                where ImageFileExtensions.Contains(fi.Extension)
                select fi.FullName
            );
        }

        public void RemoveFiles(IEnumerable<string> files)
        {
            Files.RemoveRange(files);
        }

        public void RemoveSelectedFiles()
        {
            Files.RemoveRange(SelectedFiles);
            SelectedFiles.Clear();
        }

        public void SelectFile(string file)
        {
            try
            {
                SelectedFiles.Add(file);
            }
            catch (ArgumentException)
            {
                // ignore exception about duplicate key
            }
        }

        public void DeselectFile(string file)
        {
            SelectedFiles.Remove(file);
        }
    }
}
