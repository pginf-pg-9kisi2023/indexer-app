using System;

using Indexer.Model;

namespace Indexer.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public string ProgramName { get; private set; } = "Indexer";
        private Session? _session;
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
            _session = new Session(Config.FromFile(configFilePath));
        }

        public void OpenSession(string sessionFilePath)
        {
            _session = Session.FromFile(sessionFilePath);
        }

        public void SaveSession()
        {
            if (_session is null)
            {
                throw new InvalidOperationException("No session is open.");
            }

            _session.Save();
        }

        public void CloseSession()
        {
            _session = null;
        }
    }
}
