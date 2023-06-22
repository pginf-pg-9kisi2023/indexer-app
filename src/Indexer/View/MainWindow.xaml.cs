using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        public void OpenSession(string sessionLocation)
        {
            try
            {
                Data.OpenSession(sessionLocation);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(
                    owner: this,
                    caption: Data.ProgramName,
                    messageBoxText: (
                        "Program nie może znaleźć pliku o podanej ścieźce:\n"
                        + sessionLocation
                    ),
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error
                );
                return;
            }
            catch (SerializationException ex)
            {
                ShowSerializationExceptionDialog(ex);
                return;
            }
            Data.SetTemporaryStatusOverride($"Wczytano sesję z '{sessionLocation}'");
            MainImage.InvalidateMeasure();
            MainImage.UpdateLayout();
            ScrollCurrentLabelIntoCenter();
        }

        private void CanExecute_IsSessionOpen(
            object sender, CanExecuteRoutedEventArgs e
        )
        {
            e.CanExecute = Data.IsSessionOpen;
        }

        private void CanExecute_IsSessionModified(
            object sender, CanExecuteRoutedEventArgs e
        )
        {
            e.CanExecute = Data.IsSessionModified;
        }

        private void CanExecute_HasExportedBefore(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Data.HasExportedBefore;
        }

        private void CanExecute_HasImages(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Data.HasImages;
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
                return;
            }
            Data.SetTemporaryStatusOverride("Utworzono nową sesję");
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
            OpenSession(sessionLocation);
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
            Data.SetStatus("");
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
            Data.SetTemporaryStatusOverride(
                $"Zapisano sesję do '{Data.SessionFilePath}'"
            );
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
                var centerScrollViewerAfter = Data.IndexedImages.Count == 0;
                Data.AddIndexedImages(imageSelection.Files);
                if (Data.CurrentLabel != null)
                {
                    Data.SetStatus($"Zaznaczanie etykiety '{Data.CurrentLabel.Name}'");
                }
                else
                {
                    Data.SetStatus("");
                }
                Data.SetTemporaryStatusOverride(
                    $"Dodano {imageSelection.Files.Count} zdjęć"
                );
                if (centerScrollViewerAfter)
                {
                    ScrollCurrentLabelIntoCenter();
                }
            }
            else
            {
                return;
            }
        }

        private void ExportTo_Click(object sender, RoutedEventArgs e)
        {
            Data.ExportToLastFile();
            Data.SetTemporaryStatusOverride(
                $"Wyeksportowano punkty do '{Data.LastExportPath}'"
            );
        }

        private void ExportAsCSV_Click(object sender, RoutedEventArgs e)
        {
            var fileLocation = PromptForExportLocation("csv");
            if (fileLocation is null)
            {
                return;
            }

            Data.ExportPointsToCSV(fileLocation);
            Data.SetTemporaryStatusOverride(
                $"Wyeksportowano punkty do '{fileLocation}'"
            );
        }

        private void ExportAsXML_Click(object sender, RoutedEventArgs e)
        {
            var fileLocation = PromptForExportLocation("xml");
            if (fileLocation == null)
            {
                return;
            }

            Data.ExportPointsToXML(fileLocation);
            Data.SetTemporaryStatusOverride(
                $"Wyeksportowano punkty do '{fileLocation}'"
            );
        }

        private string? PromptForExportLocation(string fileExt)
        {
            string? path = null;
            Data.LastExportPaths.TryGetValue(fileExt, out path);
            var saveFileDialog = new SaveFileDialog()
            {
                Title = $"Eksportuj do pliku {fileExt.ToUpperInvariant()}",
                DefaultExt = fileExt,
                Filter = $"Pliki {fileExt.ToUpperInvariant()}|*.{fileExt}"
            };
            if (path is not null)
            {
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(path);
                saveFileDialog.FileName = Path.GetFileName(path);
            }
            else
            {
                saveFileDialog.RestoreDirectory = true;
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }

        private void ShortHelp_Click(object sender, RoutedEventArgs e)
        {
            var bullet = "\u2022";
            var arrows = "\u2191/\u2193/\u2192/\u2190";
            MessageBox.Show(
                owner: this,
                caption: Data.ProgramName,
                messageBoxText: $"""
                Etykietowanie zdjęć:
                {bullet} Przesuń etykietę do lokalizacji kursora myszy: Lewy przycisk myszy
                {bullet} Usuń współrzędne obecnej etykiety: Prawy przycisk myszy/Delete
                {bullet} Przesuń etykietę o 1 piksel: {arrows}
                {bullet} Przesuń etykietę o 4 piksele: Ctrl+{arrows}
                {bullet} Przesuń etykietę o 25 pikseli: Shift+{arrows}
                {bullet} Przesuń etykietę o 100 pikseli: Ctrl+Shift+{arrows}

                Nawigacja pomiędzy zdjęciami/punktami:
                {bullet} Przejdź do wyboru następnego punktu: Enter
                {bullet} Przejdź do następnego zdjęcia: Ctrl+Tab
                {bullet} Przejdź do poprzedniego zdjęcia: Ctrl+Shift+Tab

                Lupa:
                {bullet} Przybliż lupę: +
                {bullet} Oddal lupę: -
                {bullet} Ustaw lupę na 100%, 200%, ..., 500%: 1, 2, 3, 4, 5
                
                Rozmiar zdjęcia:
                {bullet} Dopasowane do ekranu: Ctrl+0
                {bullet} Rzeczywisty rozmiar: Ctrl+1

                Główne menu:
                {bullet} Utwórz nową sesję: Ctrl+N
                {bullet} Wczytaj sesję: Ctrl+O
                {bullet} Dodaj zdjęcia i/lub foldery: Ctrl+I
                {bullet} Zapisz sesję: Ctrl+S
                {bullet} Zapisz sesji jako...: Ctrl+Shift+S
                {bullet} Wyeksportuj sesję (do ostatniego pliku eksportu): Alt+C
                {bullet} Wyeksportuj sesję jako plik CSV: Alt+C
                {bullet} Wyeksportuj sesję jako plik XML: Alt+X
                {bullet} Zamknij bieżącą sesję: Ctrl+W
                {bullet} Zamknij aplikację: Alt+F4
                {bullet} Wyświetl pomoc dot. skrótów klawiaturowych: Ctrl+F1
                """,
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
            Point mousePos = e.GetPosition(MainImage.Image);
            var x = mousePos.X * Data.CurrentImage!.Width / MainImage.Image.ActualWidth;
            var y = mousePos.Y * Data.CurrentImage!.Height / MainImage.Image.ActualHeight;
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
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                Data.RemoveCurrentLabelPosition();
            }
        }

        private void PreviousImageButton_Click(object sender, RoutedEventArgs e)
        {
            Data.SwitchToPreviousImage();
            Data.SetTemporaryStatusOverride($"Załadowano '{Data.CurrentImage!.Path}'");
            ScrollCurrentLabelIntoCenter();
        }

        private void NextImageButton_Click(object sender, RoutedEventArgs e)
        {
            Data.SwitchToNextImage();
            Data.SetTemporaryStatusOverride($"Załadowano '{Data.CurrentImage!.Path}'");
            ScrollCurrentLabelIntoCenter();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            var multiplier = 1;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.D0:
                    case Key.NumPad0:
                        FitImageOnScreen();
                        return;
                    case Key.D1:
                    case Key.NumPad1:
                        ShowActualImageSize();
                        return;
                }
                multiplier *= 4;
            }
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                multiplier *= 25;
            }
            switch (e.Key)
            {
                case Key.OemPlus:
                case Key.Add:
                    Magnifier.Value += 1;
                    break;
                case Key.OemMinus:
                case Key.Subtract:
                    Magnifier.Value -= 1;
                    break;
                case Key.D1:
                case Key.NumPad1:
                    Magnifier.Value = 1;
                    break;
                case Key.D2:
                case Key.NumPad2:
                    Magnifier.Value = 2;
                    break;
                case Key.D3:
                case Key.NumPad3:
                    Magnifier.Value = 3;
                    break;
                case Key.D4:
                case Key.NumPad4:
                    Magnifier.Value = 4;
                    break;
                case Key.D5:
                case Key.NumPad5:
                    Magnifier.Value = 5;
                    break;
                case Key.Up:
                    Data.MoveCurrentLabelPositionRelatively(y: multiplier * -1);
                    ScrollCurrentLabelIntoCenter();
                    break;
                case Key.Down:
                    Data.MoveCurrentLabelPositionRelatively(y: multiplier * 1);
                    ScrollCurrentLabelIntoCenter();
                    break;
                case Key.Left:
                    Data.MoveCurrentLabelPositionRelatively(x: multiplier * -1);
                    ScrollCurrentLabelIntoCenter();
                    break;
                case Key.Right:
                    Data.MoveCurrentLabelPositionRelatively(x: multiplier * 1);
                    ScrollCurrentLabelIntoCenter();
                    break;
                case Key.Enter:
                    Data.SwitchToNextLabel();
                    Data.SetStatus($"Zaznaczanie etykiety '{Data.CurrentLabel!.Name}'");
                    ScrollCurrentLabelIntoCenter();
                    break;
                case Key.Delete:
                    Data.RemoveCurrentLabelPosition();
                    break;
            }
        }

        private void ShowActualSizeButton_Checked(object sender, RoutedEventArgs e)
        {
            ShowActualImageSize();
        }

        private void ShowActualSizeButton_Unchecked(object sender, RoutedEventArgs e)
        {
            FitImageOnScreen();
        }

        private void ShowActualImageSize()
        {
            ShowActualSizeButton.IsChecked = true;
            MainImage.Stretch = Stretch.None;
            MainImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            MainImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            MainImage.InvalidateMeasure();
            MainImage.UpdateLayout();
            ScrollCurrentLabelIntoCenter();
        }

        private void FitImageOnScreen()
        {
            ShowActualSizeButton.IsChecked = false;
            MainImage.Stretch = Stretch.Uniform;
            MainImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            MainImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        private void MainImageScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // enable horizontal scrolling when Shift key is held
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                MainImageScrollViewer.ScrollToHorizontalOffset(
                    MainImageScrollViewer.HorizontalOffset - e.Delta
                );
                e.Handled = true;
            }
        }

        private void MainImageScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0 || e.VerticalChange != 0)
            {
                // we're not interested in scrollbar movement (ours or not)
                return;
            }
            if (e.ViewportWidthChange != 0)
            {
                var newWidth = e.ViewportWidth;
                var oldWidth = newWidth - e.ViewportWidthChange;
                if (oldWidth > 0 && newWidth > 0)
                {
                    var x = MainImageScrollViewer.HorizontalOffset + oldWidth / 2;
                    ScrollXIntoCenter(x);
                }
            }
            if (e.ViewportHeightChange != 0)
            {
                var newHeight = e.ViewportHeight;
                var oldHeight = newHeight - e.ViewportHeightChange;
                if (oldHeight > 0 && newHeight > 0)
                {
                    var y = MainImageScrollViewer.VerticalOffset + oldHeight / 2;
                    ScrollYIntoCenter(y);
                }
            }
        }

        private void ScrollCurrentLabelIntoCenter()
        {
            if (Data.CurrentLabel?.Position == null)
            {
                return;
            }
            double x = Data.CurrentLabel.X;
            x *= MainImageScrollViewer.ExtentWidth;
            x /= MainImage.BitmapSource!.PixelWidth;
            double y = Data.CurrentLabel.Y;
            y *= MainImageScrollViewer.ExtentHeight;
            y /= MainImage.BitmapSource!.PixelHeight;
            ScrollXIntoCenter(x);
            ScrollYIntoCenter(y);
        }

        private void ScrollXIntoCenter(double x)
        {
            MainImageScrollViewer.ScrollToHorizontalOffset(
                x - MainImageScrollViewer.ViewportWidth / 2
            );
        }

        private void ScrollYIntoCenter(double y)
        {
            MainImageScrollViewer.ScrollToVerticalOffset(
                y - MainImageScrollViewer.ViewportHeight / 2
            );
        }

        private void FilesListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item is null)
            {
                return;
            }
            var indexedImage = item.Content as IndexedImageViewModel;
            if (indexedImage is not null)
            {
                Data.SetCurrentImage(indexedImage);
                Data.SetTemporaryStatusOverride(
                    $"Załadowano '{indexedImage.ImagePath}'"
                );
                ScrollCurrentLabelIntoCenter();
            }
        }

        private void PointsListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item is null)
            {
                return;
            }
            var label = item.Content as LabelViewModel;
            if (label is not null)
            {
                Data.SetCurrentLabel(label);
                Data.SetStatus($"Zaznaczanie etykiety '{label.Name}'");
                ScrollCurrentLabelIntoCenter();
            }
        }

        private void AnalyzeImages_Click(object sender, ExecutedRoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Title = "Wybierz program do wsadowej analizy zdjęć",
                DefaultExt = "exe",
                Filter = "Pliki wykonywalne|*.exe",
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            Data.AnalyzeImages(openFileDialog.FileName);
            Data.SetTemporaryStatusOverride("Wsadowa analiza zdjęć ukończona");
        }
    }
}
