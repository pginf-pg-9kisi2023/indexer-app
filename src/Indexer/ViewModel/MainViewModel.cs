using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Timers;

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
        public bool HasExportedBefore => LastExportType != null;
        public string? LastExportType { get; private set; }
        private readonly Dictionary<string, string> _LastExportPaths = new();
        public ReadOnlyDictionary<string, string> LastExportPaths =>
            new(_LastExportPaths);
        public string? LastExportPath =>
            LastExportType != null ? LastExportPaths[LastExportType] : null;
        public string? LastExportFileName
        {
            get
            {
                if (LastExportType == null)
                {
                    return null;
                }
                return Path.GetFileName(LastExportPaths[LastExportType]);
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
        public MemoryStream? CurrentBitmapImage => CurrentImage?.LoadedImage;
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
        public HintViewModel? CurrentHint { get; private set; }
        public ImageViewModel? CurrentHintImage { get; private set; }
        public MemoryStream? CurrentHintBitmapImage => CurrentHint?.Image?.LoadedImage;
        public LabelViewModel? CurrentLabel =>
            CurrentHint is null ? null : CurrentLabels[CurrentHint.Name];
        public bool HasImages => _session?.CurrentImageIndex != null;
        public Point? ImageCursorPosition { get; private set; }
        public string ImageCursorPositionText
        {
            get
            {
                if (ImageCursorPosition is Point pos)
                {
                    return $"{pos.X}, {pos.Y}";
                }
                return "";
            }
        }
        public Point? SavedPosition => CurrentLabel?.Position;
        public string SavedPositionText
        {
            get
            {
                var currentLabel = CurrentLabel;
                if (SavedPosition is Point pos)
                {
                    return $"{pos.X}, {pos.Y}";
                }
                return "";
            }
        }
        private string _statusText = "";
        private Timer? _statusTimer;
        public string StatusText { get; private set; } = "";
        private bool _isCurrentLabelAutoCentered = true;
        public bool IsCurrentLabelAutoCentered
        {
            get => _isCurrentLabelAutoCentered;
            set
            {
                _isCurrentLabelAutoCentered = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel() { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _statusTimer?.Dispose();
                _statusTimer = null;
            }
        }

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
            IndexedImages.Clear();
            if (_session != null)
            {
                IndexedImages.AddRange(
                    from indexedImage in _session.IndexedImages
                    select new IndexedImageViewModel(_session, indexedImage)
                );
                if (IndexedImages.Count != 0)
                {
                    SetCurrentImageIndex(_session.CurrentImageIndex, desynced: true);
                }
            }
            else
            {
                CurrentImage?.UnloadImage();
                CurrentHintImage?.UnloadImage();
                CurrentImage = null;
                CurrentHint = null;
                CurrentHintImage = null;
            }
            LastExportType = null;
            _LastExportPaths.Clear();
            IsSessionModified = isSessionModified;
            OnPropertyChanged(nameof(IsSessionOpen));
            OnPropertyChanged(nameof(IsSessionOnDisk));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(SessionFilePath));
            OnPropertyChanged(nameof(SessionFileDirectory));
            OnPropertyChanged(nameof(SessionFileName));
            OnPropertyChanged(nameof(SessionFileTitle));
            OnPropertyChanged(nameof(IndexedImages));
            OnPropertyChanged(nameof(CurrentIndexedImage));
            OnPropertyChanged(nameof(CurrentImage));
            OnPropertyChanged(nameof(CurrentBitmapImage));
            OnPropertyChanged(nameof(CurrentLabel));
            OnPropertyChanged(nameof(CurrentLabels));
            OnPropertyChanged(nameof(SavedPosition));
            OnPropertyChanged(nameof(SavedPositionText));
            OnPropertyChanged(nameof(CurrentHint));
            OnPropertyChanged(nameof(CurrentHintImage));
            OnPropertyChanged(nameof(CurrentHintBitmapImage));
            OnPropertyChanged(nameof(HasImages));
            OnPropertyChanged(nameof(LastExportType));
            OnPropertyChanged(nameof(LastExportPath));
            OnPropertyChanged(nameof(LastExportFileName));
            OnPropertyChanged(nameof(HasExportedBefore));
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
                var indexedImageVM = new IndexedImageViewModel(_session, indexedImage);
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
                    OnPropertyChanged(nameof(HasImages));
                }
            }
        }

        public void SwitchToPreviousImage()
        {
            if (_session is null)
            {
                throw new InvalidOperationException("No session is open.");
            }

            if (_session.CurrentImageIndex is int idx)
            {
                if (idx == 0)
                {
                    idx = IndexedImages.Count;
                }
                SetCurrentImageIndex(idx - 1);
            }
        }

        public void SwitchToNextImage()
        {
            if (_session is null)
            {
                throw new InvalidOperationException("No session is open.");
            }

            if (_session.CurrentImageIndex is int idx)
            {
                SetCurrentImageIndex((idx + 1) % IndexedImages.Count);
            }
        }

        public void SetCurrentImage(IndexedImageViewModel indexedImage)
        {
            SetCurrentImageIndex(IndexedImages.IndexOf(indexedImage));
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
                var oldHintImage = CurrentHintImage;
                _session.CurrentImageIndex = idx;
                CurrentImage = CurrentIndexedImage?.Image;
                CurrentImage?.LoadImage();
                oldImage?.UnloadImage();
                CurrentHint = new(_session.CurrentHint!);
                CurrentHintImage = CurrentHint.Image;
                CurrentHintImage?.LoadImage();
                oldHintImage?.UnloadImage();
                IsSessionModified = true;
                OnPropertyChanged(nameof(CurrentIndexedImage));
                OnPropertyChanged(nameof(CurrentImage));
                OnPropertyChanged(nameof(CurrentBitmapImage));
                OnPropertyChanged(nameof(CurrentLabel));
                OnPropertyChanged(nameof(CurrentLabels));
                OnPropertyChanged(nameof(SavedPosition));
                OnPropertyChanged(nameof(SavedPositionText));
                OnPropertyChanged(nameof(CurrentHint));
                OnPropertyChanged(nameof(CurrentHintImage));
                OnPropertyChanged(nameof(CurrentHintBitmapImage));
            }
        }

        public void MoveCurrentLabelPositionRelatively(int x = 0, int y = 0)
        {
            if (
                _session is null
                || CurrentIndexedImage is null
                || _session.CurrentHint is null
            )
            {
                return;
            }
            var currentLabel = CurrentLabel;
            if (currentLabel?.Position is null)
            {
                var label = new Label(_session.CurrentHint.Name);
                currentLabel = CurrentIndexedImage.AddLabel(label);
            }

            currentLabel.X = Math.Min(
                Math.Max(0, currentLabel.X + x), CurrentIndexedImage.Image.Width - 1
            );
            currentLabel.Y = Math.Min(
                Math.Max(0, currentLabel.Y + y), CurrentIndexedImage.Image.Height - 1
            );
            CurrentLabels.TriggerReset();
            IsSessionModified = true;
            OnPropertyChanged(nameof(CurrentLabel));
            OnPropertyChanged(nameof(CurrentLabels));
            OnPropertyChanged(nameof(SavedPosition));
            OnPropertyChanged(nameof(SavedPositionText));
        }

        public void SetCurrentLabelPosition(int x, int y)
        {
            if (
                _session is null
                || CurrentIndexedImage is null
                || _session.CurrentHint is null
            )
            {
                return;
            }
            var currentLabel = CurrentLabel;
            if (currentLabel?.Position is null)
            {
                var label = new Label(_session.CurrentHint.Name);
                currentLabel = CurrentIndexedImage.AddLabel(label);
            }

            currentLabel.X = x;
            currentLabel.Y = y;
            CurrentLabels.TriggerReset();
            IsSessionModified = true;
            OnPropertyChanged(nameof(CurrentLabel));
            OnPropertyChanged(nameof(CurrentLabels));
            OnPropertyChanged(nameof(SavedPosition));
            OnPropertyChanged(nameof(SavedPositionText));
        }

        public void RemoveCurrentLabelPosition()
        {
            if (
                _session is null
                || CurrentIndexedImage is null
                || _session.CurrentHint is null
            )
            {
                return;
            }
            var currentLabel = CurrentLabel;
            if (currentLabel?.Position is not null)
            {
                CurrentIndexedImage.DeleteLabel(_session.CurrentHint.Name);
            }

            IsSessionModified = true;
            OnPropertyChanged(nameof(CurrentLabel));
            OnPropertyChanged(nameof(CurrentLabels));
            OnPropertyChanged(nameof(SavedPosition));
            OnPropertyChanged(nameof(SavedPositionText));
        }

        public void SetCurrentImageCursorPosition(int x, int y)
        {
            ImageCursorPosition = new Point(x, y);
            OnPropertyChanged(nameof(ImageCursorPosition));
            OnPropertyChanged(nameof(ImageCursorPositionText));
        }

        public void ClearCurrentImageCursorPosition()
        {
            ImageCursorPosition = null;
            OnPropertyChanged(nameof(ImageCursorPosition));
            OnPropertyChanged(nameof(ImageCursorPositionText));
        }

        public void SwitchToNextLabel()
        {
            if (
                _session is null
                || CurrentIndexedImage is null
                || _session.CurrentHint is null
            )
            {
                return;
            }
            var collection = (ReadOnlyCollection<Hint>)_session.Config.Hints;
            if (collection[^1].Name == _session.CurrentHint.Name)
            {
                _session.CurrentHint = null;
                SwitchToNextImage();
                return;
            }
            var newHint = collection[collection.IndexOf(_session.CurrentHint!) + 1];

            SetCurrentHint(newHint);
        }

        public void SetCurrentLabel([NotNull] LabelViewModel label)
        {
            if (_session is null)
            {
                return;
            }
            SetCurrentHint(_session.Config.Hints[label.Name]);
        }

        private void SetCurrentHint(Hint newHint)
        {
            var oldHintImage = CurrentHintImage;
            _session!.CurrentHint = newHint;
            CurrentHint = new(_session.CurrentHint!);
            CurrentHintImage = CurrentHint.Image;
            CurrentHintImage?.LoadImage();
            oldHintImage?.UnloadImage();

            IsSessionModified = true;
            OnPropertyChanged(nameof(CurrentLabel));
            OnPropertyChanged(nameof(SavedPosition));
            OnPropertyChanged(nameof(SavedPositionText));
            OnPropertyChanged(nameof(CurrentHint));
            OnPropertyChanged(nameof(CurrentHintImage));
            OnPropertyChanged(nameof(CurrentHintBitmapImage));
        }

        public void ExportToLastFile()
        {
            if (_session is null)
            {
                throw new InvalidOperationException("No session is open.");
            }
            if (LastExportType is null || LastExportPath is null)
            {
                throw new InvalidOperationException("The file to export to is unknown.");
            }
            switch (LastExportType)
            {
                case "xml":
                    ExportPointsToXML(LastExportPath);
                    break;
                case "csv":
                    ExportPointsToCSV(LastExportPath);
                    break;
            }
        }

        public void ExportPointsToXML([NotNull] string filePath)
        {
            if (_session is null)
            {
                throw new InvalidOperationException("No session is open.");
            }
            _session.ExportPointsToXML(filePath);
            var exportType = "xml";
            _LastExportPaths[exportType] = filePath;
            LastExportType = exportType;
            OnPropertyChanged(nameof(LastExportType));
            OnPropertyChanged(nameof(LastExportPath));
            OnPropertyChanged(nameof(LastExportFileName));
            OnPropertyChanged(nameof(HasExportedBefore));
        }

        public void ExportPointsToCSV([NotNull] string filePath)
        {
            if (_session is null)
            {
                throw new InvalidOperationException("No session is open.");
            }
            _session.ExportPointsToCSV(filePath);
            var exportType = "csv";
            _LastExportPaths[exportType] = filePath;
            LastExportType = exportType;
            OnPropertyChanged(nameof(LastExportType));
            OnPropertyChanged(nameof(LastExportPath));
            OnPropertyChanged(nameof(LastExportFileName));
            OnPropertyChanged(nameof(HasExportedBefore));
        }

        public void AnalyzeImages([NotNull] string filePath)
        {
            if (_session is null)
            {
                throw new InvalidOperationException("No session is open.");
            }
            _session.AnalyzeImages(filePath);
            IndexedImages.Clear();
            IndexedImages.AddRange(
                from indexedImage in _session.IndexedImages
                select new IndexedImageViewModel(_session, indexedImage)
            );
            if (IndexedImages.Count != 0)
            {
                SetCurrentImageIndex(_session.CurrentImageIndex, desynced: true);
            }
            IsSessionModified = true;
            OnPropertyChanged(nameof(IndexedImages));
            OnPropertyChanged(nameof(CurrentBitmapImage));
            OnPropertyChanged(nameof(CurrentLabels));
            OnPropertyChanged(nameof(CurrentLabel));
            OnPropertyChanged(nameof(CurrentIndexedImage));
            OnPropertyChanged(nameof(CurrentHintImage));
        }

        public void SetStatus(string text)
        {
            _statusText = text;
            if (!_statusTimer?.Enabled ?? true)
            {
                StatusText = text;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public void SetTemporaryStatusOverride(string text)
        {
            _statusTimer?.Stop();
            _statusTimer?.Dispose();

            StatusText = text;
            OnPropertyChanged(nameof(StatusText));
            _statusTimer = new Timer(8000);
            _statusTimer.Elapsed += OnStatusTimerElapsed;
            _statusTimer.Start();
        }

        private void OnStatusTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            _statusTimer?.Stop();
            _statusTimer?.Dispose();
            _statusTimer = null;
            StatusText = _statusText;
            OnPropertyChanged(nameof(StatusText));
        }
    }
}
