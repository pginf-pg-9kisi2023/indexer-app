using System;
using System.IO;

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
            OnPropertyChanged(nameof(IsSessionOpen));
            OnPropertyChanged(nameof(IsSessionOnDisk));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(SessionFilePath));
            OnPropertyChanged(nameof(SessionFileDirectory));
            OnPropertyChanged(nameof(SessionFileName));
            OnPropertyChanged(nameof(SessionFileTitle));
        }
    }
}
