using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using VXMusicDesktop.Core;
using System.Windows.Input;
using System.ComponentModel;
using System.IO;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class AboutViewModel : INotifyPropertyChanged
    {
        private RelayCommand openFolderClick;
        public ICommand OpenFolderCommand => openFolderClick ??= new RelayCommand(OpenFolder);

        public event PropertyChangedEventHandler? PropertyChanged;
        
        private static readonly string LogsOutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VirtualXtensions", "VXMusic", "Logs");
        
        public AboutViewModel()
        {
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void OpenFolder(object commandParameter)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LogsOutputPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Log folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
