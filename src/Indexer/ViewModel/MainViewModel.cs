using System;

using Indexer.Model;

namespace Indexer.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        private Session? _session;
        public bool IsSessionOpen => _session != null;
        public bool IsSessionOnDisk => _session != null && _session.FilePath != null;

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
