using System.Windows;

using Indexer.View;

namespace Indexer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var window = new MainWindow();
            window.Show();
            if (e.Args.Length > 0)
            {
                window.OpenSession(e.Args[0]);
            }
        }
    }
}
