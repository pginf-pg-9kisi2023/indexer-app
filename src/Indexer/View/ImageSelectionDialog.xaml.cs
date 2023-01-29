using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Indexer.ViewModel;

using Microsoft.Win32;

namespace Indexer.View
{
    public partial class ImageSelectionDialog : Window
    {
        private ImageSelectionViewModel Data => (ImageSelectionViewModel)DataContext;

        public ImageSelectionDialog(ImageSelectionViewModel dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            var filterPattern = String.Join(
                ";", Data.ImageFileExtensions.Select(ext => $"*{ext}")
            );
            var openFileDialog = new OpenFileDialog()
            {
                Title = "Wybierz pliki ze zdjęciami",
                Filter = $"Pliki obrazów|{filterPattern}|Wszystkie pliki|*.*",
                Multiselect = true,
                CheckFileExists = false,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Data.AddFiles(openFileDialog.FileNames);
            }
        }

        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            using var openFolderDialog = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = "Wybierz folder ze zdjęciami",
                UseDescriptionForTitle = true
            };
            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Data.AddFolder(openFolderDialog.SelectedPath);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Data.RemoveSelectedFiles();
        }

        private void FileCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox != null)
            {
                var file = checkbox.DataContext as string;
                if (file != null)
                {
                    Data.SelectFile(file);
                }
            }
        }

        private void FileCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox != null)
            {
                var file = checkbox.DataContext as string;
                if (file != null)
                {
                    Data.DeselectFile(file);
                }
            }
        }
    }
}
