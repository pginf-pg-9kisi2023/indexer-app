using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

using Indexer.Collections;
using Indexer.Model;

namespace Indexer.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public string ProgramName { get; private set; } = "Indexer";
        private Session? _session;
        public string? SessionFilePath => _session?.FilePath;
        public string? SessionFileDirectory
        {
            get
            {
                if (_session?.FilePath == null)
                {
                    return null;
                }

                return Path.GetDirectoryName(_session.FilePath);
            }
        }
        public string? SessionFileName
        {
            get
            {
                if (_session?.FilePath == null)
                {
                    return null;
                }

                return Path.GetFileName(_session.FilePath);
            }
        }
        public string SessionFileTitle => _session?.FilePath ?? "Bez tytułu";
        public bool IsSessionOpen => _session != null;
        public bool IsSessionOnDisk => _session != null && _session.FilePath != null;
        private bool _isSessionModified;
        public bool IsSessionModified
        {
            get => _isSessionModified;
            private set
            {
                if (_isSessionModified != value)
                {
                    _isSessionModified = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        public string Title
        {
            get
            {
                if (_session is null)
                {
                    return ProgramName;
                }
                var title = $"{SessionFileTitle} - {ProgramName}";
                if (IsSessionModified)
                {
                    title = $"*{title}";
                }
                return title;
            }
        }
        public IndexedImageVMObservableCollection IndexedImages { get; private set; }
            = new();
        public IndexedImageViewModel? CurrentIndexedImage
        {
            get
            {
                var collection = (Collection<IndexedImageViewModel>)IndexedImages;
                if (_session?.CurrentImageIndex is int idx)
                {
                    return collection[idx];
                }
                return null;
            }
        }
        public ImageViewModel? CurrentImage { get; private set; }
        public BitmapSource? CurrentBitmapImage => CurrentImage?.LoadedImage;
        public LabelVMObservableCollection CurrentLabels
        {
            get
            {
                var collection = (Collection<IndexedImageViewModel>)IndexedImages;
                if (_session?.CurrentImageIndex is int idx)
                {
                    return collection[idx].Labels;
                }
                return new();
            }
        }

        public MainViewModel() { }

        public void NewSession(string configFilePath)
        {
            SetSession(
                new Session(Config.FromFile(configFilePath)), isSessionModified: true
            );
        }

        public void OpenSession(string sessionFilePath)
        {
            SetSession(Session.FromFile(sessionFilePath), isSessionModified: false);
        }

        public void SaveSession(string? sessionFilePath = null)
        {
            if (_session is null)
            {
                throw new InvalidOperationException("No session is open.");
            }

            if (sessionFilePath != null)
            {
                _session.FilePath = sessionFilePath;
                OnPropertyChanged(nameof(SessionFilePath));
                OnPropertyChanged(nameof(SessionFileDirectory));
                OnPropertyChanged(nameof(SessionFileName));
                OnPropertyChanged(nameof(SessionFileTitle));
                OnPropertyChanged(nameof(Title));
            }

            _session.Save();
            IsSessionModified = false;
        }

        public void CloseSession()
        {
            SetSession(null, isSessionModified: false);
        }

        private void SetSession(Session? value, bool isSessionModified)
        {
            _session = value;
            IsSessionModified = isSessionModified;
            IndexedImages.Clear();
            if (_session != null)
            {
                IndexedImages.AddRange(
                    from indexedImage in _session.IndexedImages
                    select new IndexedImageViewModel(indexedImage)
                );
                if (IndexedImages.Count != 0)
                {
                    SetCurrentImageIndex(_session.CurrentImageIndex, desynced: true);
                }
            }
            OnPropertyChanged(nameof(IsSessionOpen));
            OnPropertyChanged(nameof(IsSessionOnDisk));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(SessionFilePath));
            OnPropertyChanged(nameof(SessionFileDirectory));
            OnPropertyChanged(nameof(SessionFileName));
            OnPropertyChanged(nameof(SessionFileTitle));
            OnPropertyChanged(nameof(IndexedImages));
            OnPropertyChanged(nameof(CurrentImage));
            OnPropertyChanged(nameof(CurrentBitmapImage));
            OnPropertyChanged(nameof(CurrentLabels));
        }

        public void AddIndexedImages([NotNull] IEnumerable<string> imagePaths)
        {
            if (_session is null)
            {
                throw new InvalidOperationException("No session is open.");
            }

            List<IndexedImageViewModel> toAdd = new();
            foreach (var path in imagePaths)
            {
                var indexedImage = new IndexedImage(path);
                try
                {
                    _session.AddIndexedImage(indexedImage);
                }
                catch (ArgumentException)
                {
                    continue;
                }
                var indexedImageVM = new IndexedImageViewModel(indexedImage);
                toAdd.Add(indexedImageVM);
            }
            var wasEmpty = IndexedImages.Count == 0;
            IndexedImages.AddRange(toAdd);
            if (IndexedImages.Count != 0)
            {
                IsSessionModified = true;
                OnPropertyChanged(nameof(IndexedImages));
                if (wasEmpty)
                {
                    SetCurrentImageIndex(0, desynced: true);
                }
            }
        }

        private void SetCurrentImageIndex(int? idx, bool desynced = false)
        {
            if (_session is null)
            {
                throw new InvalidOperationException("No session is open.");
            }

            if (desynced || _session.CurrentImageIndex != idx)
            {
                var oldImage = CurrentImage;
                _session.CurrentImageIndex = idx;
                CurrentImage = CurrentIndexedImage?.Image;
                CurrentImage?.LoadImage();
                oldImage?.UnloadImage();
                IsSessionModified = true;
                OnPropertyChanged(nameof(CurrentIndexedImage));
                OnPropertyChanged(nameof(CurrentImage));
                OnPropertyChanged(nameof(CurrentBitmapImage));
                OnPropertyChanged(nameof(CurrentLabels));
            }
        }
    }
}
