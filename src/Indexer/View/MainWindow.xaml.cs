using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

using Indexer.ViewModel;

using Microsoft.Win32;

namespace Indexer.View
{
    public partial class MainWindow : Window
    {
        private MainViewModel Data => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateSession_Click(object sender, RoutedEventArgs e)
        {
            if (!PromptAboutUnsavedChanges())
            {
                return;
            }
            var configLocation = PromptForConfigLocation();
            if (configLocation is null)
            {
                return;
            }
            try
            {
                Data.NewSession(configLocation);
            }
            catch (SerializationException ex)
            {
                ShowSerializationExceptionDialog(ex);
            }
        }

        private void LoadSession_Click(object sender, RoutedEventArgs e)
        {
            if (!PromptAboutUnsavedChanges())
            {
                return;
            }
            var sessionLocation = PromptForSessionLocation(saving: false);
            if (sessionLocation is null)
            {
                return;
            }
            try
            {
                Data.OpenSession(sessionLocation);
            }
            catch (SerializationException ex)
            {
                ShowSerializationExceptionDialog(ex);
            }
        }

        private void SaveSession_Click(object sender, RoutedEventArgs e)
        {
            SaveSession();
        }

        private void SaveSessionAs_Click(object sender, RoutedEventArgs e)
        {
            SaveSession(alwaysPrompt: true);
        }

        private void CloseSession_Click(object sender, RoutedEventArgs e)
        {
            if (!PromptAboutUnsavedChanges())
            {
                return;
            }
            Data.CloseSession();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!PromptAboutUnsavedChanges())
            {
                e.Cancel = true;
                return;
            }
            Data.CloseSession();
        }

        private bool PromptAboutUnsavedChanges()
        {
            if (!Data.IsSessionModified)
            {
                return true;
            }
            var sessionFileTitle = Data.SessionFileTitle;
            switch (
                MessageBox.Show(
                    owner: this,
                    caption: "Zapisać zmiany?",
                    messageBoxText: $"Czy chcesz zapisać zmiany w pliku {sessionFileTitle}?",
                    button: MessageBoxButton.YesNoCancel,
                    icon: MessageBoxImage.Warning,
                    defaultResult: MessageBoxResult.Yes
                )
            )
            {
                case MessageBoxResult.Yes:
                    return SaveSession();
                case MessageBoxResult.No:
                    return true;
                case MessageBoxResult.Cancel:
                    return false;
            }
            throw new UnreachableException();
        }

        private bool SaveSession(bool alwaysPrompt = false)
        {
            string? saveLocation = null;
            if (alwaysPrompt || !Data.IsSessionOnDisk)
            {
                saveLocation = PromptForSessionLocation(saving: true);
                if (saveLocation is null)
                {
                    return false;
                }
            }
            Data.SaveSession(saveLocation);
            return true;
        }

        private void ShowSerializationExceptionDialog(SerializationException ex)
        {
            MessageBox.Show(
                owner: this,
                caption: Data.ProgramName,
                messageBoxText: (
                    "Program nie może odczytać tego pliku, ponieważ nie jest to"
                    + " obsługiwany typ pliku lub jest on uszkodzony.\n\n"
                    + "Szczegóły o błędzie:\n"
                    + ex.Message
                ),
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error
            );
        }

        private string? PromptForSessionLocation(bool saving)
        {
            FileDialog fileDialog;
            string title = "";
            if (saving)
            {
                fileDialog = new SaveFileDialog();
                title = "Zapisz sesję";
            }
            else
            {
                fileDialog = new OpenFileDialog();
                title = "Otwórz sesję";
            }
            fileDialog.Title = title;
            fileDialog.DefaultExt = "ixr";
            fileDialog.Filter = (
                "Pliki sesji programu Indexer|*.ixr|Wszystkie pliki|*.*"
            );
            if (Data.IsSessionOnDisk)
            {
                fileDialog.InitialDirectory = Data.SessionFileDirectory;
                if (saving)
                {
                    fileDialog.FileName = Data.SessionFileName;
                }
            }
            else
            {
                fileDialog.RestoreDirectory = true;
            }

            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileName;
            }
            return null;
        }

        private static string? PromptForConfigLocation()
        {
            var openFileDialog = new OpenFileDialog()
            {
                Title = "Wybierz plik konfiguracyjny",
                DefaultExt = "xml",
                Filter = (
                    "Pliki konfiguracyjne programu Indexer|*.xml|Wszystkie pliki|*.*"
                ),
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        private void AddImageOrFolder_Click(object sender, RoutedEventArgs e)
        {
            using var imageSelection = new ImageSelectionViewModel();
            var dialog = new ImageSelectionDialog(imageSelection);
            if (dialog.ShowDialog() == true)
            {
                Data.AddIndexedImages(imageSelection.Files);
            }
            else
            {
                return;
            }
        }

        private void ExportAsCSV_Click(object sender, RoutedEventArgs e)
        {
            var fileLocation = PromptForExportLocation("csv");
            if (fileLocation is null)
            {
                return;
            }

            Data.ExportPointsToCSV(fileLocation);
        }

        private void ExportAsXML_Click(object sender, RoutedEventArgs e)
        {
            var fileLocation = PromptForExportLocation("xml");
            if (fileLocation == null)
            {
                return;
            }

            Data.ExportPointsToXML(fileLocation);
        }

        private static string? PromptForExportLocation(string fileExt)
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Title = $"Eksportuj do pliku {fileExt.ToUpperInvariant()}",
                DefaultExt = fileExt,
                Filter = $"Pliki {fileExt.ToUpperInvariant()}|*.{fileExt}",
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }

        private void ShortcutsHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                owner: this,
                caption: Data.ProgramName,
                messageBoxText: "",
                button: MessageBoxButton.OK
            );
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown version";
            MessageBox.Show(
                owner: this,
                caption: Data.ProgramName,
                messageBoxText: (
                    $"{Data.ProgramName} {version}\n\n"
                    + "Copyright (c) 2022-2023 Jakub Kuczys, Mikołaj Morozowski,"
                    + " Mateusz Kozak, Mikołaj Nadzieja, Dawid Łydka"
                ),
                button: MessageBoxButton.OK
            );
        }

        private Point GetImageCursorPosition(MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(MainImage);
            var x = mousePos.X * MainImage.Source.Width / MainImage.ActualWidth;
            var y = mousePos.Y * MainImage.Source.Width / MainImage.ActualWidth;
            return new Point((int)x, (int)y);
        }

        private void MainImage_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = GetImageCursorPosition(e);
            Data.SetCurrentImageCursorPosition((int)pos.X, (int)pos.Y);
            this.Cursor = Cursors.Cross;
        }

        private void MainImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Data.ClearCurrentImageCursorPosition();
            this.Cursor = Cursors.Arrow;
        }

        private void MainImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = GetImageCursorPosition(e);
                Data.SetCurrentLabelPosition((int)pos.X, (int)pos.Y);
                Data.DrawCross((int)pos.X, (int)pos.Y);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                Data.RemoveCurrentLabelPosition();
                Data.ChangeCurrentImageToPrevious();
            }
        }

        private void PreviousPictureButton_Click(object sender, RoutedEventArgs e)
        {
            Data.SwitchToPreviousImage();
        }

        private void NextPictureButton_Click(object sender, RoutedEventArgs e)
        {
            Data.SwitchToNextImage();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    Data.MoveCurrentLabelPositionRelatively(y: -1);
                    if (Data.CurrentLabel is not null)
                    {
                        Data.DrawCross(Data.CurrentLabel.X, Data.CurrentLabel.Y);
                    }
                    break;
                case Key.Down:
                    Data.MoveCurrentLabelPositionRelatively(y: 1);
                    if (Data.CurrentLabel is not null)
                    {
                        Data.DrawCross(Data.CurrentLabel.X, Data.CurrentLabel.Y);
                    }
                    break;
                case Key.Left:
                    Data.MoveCurrentLabelPositionRelatively(x: -1);
                    if (Data.CurrentLabel is not null)
                    {
                        Data.DrawCross(Data.CurrentLabel.X, Data.CurrentLabel.Y);
                    }
                    break;
                case Key.Right:
                    Data.MoveCurrentLabelPositionRelatively(x: 1);
                    if (Data.CurrentLabel is not null)
                    {
                        Data.DrawCross(Data.CurrentLabel.X, Data.CurrentLabel.Y);
                    }
                    break;
                case Key.Enter:
                    Data.DrawNewLabel(Data.CurrentLabel.X, Data.CurrentLabel.Y);
                    Data.SwitchToNextLabel();
                    return;
            }
        }
    }
}
