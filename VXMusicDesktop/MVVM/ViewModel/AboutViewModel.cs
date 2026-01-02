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
using VXMusicDesktop.Core;
using Microsoft.Extensions.DependencyInjection;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class AboutViewModel : INotifyPropertyChanged
    {
        private RelayCommand openFolderClick;
        private RelayCommand testHapticClick;
        
        public ICommand OpenFolderCommand => openFolderClick ??= new RelayCommand(OpenFolder);
        public ICommand TestHapticCommand => testHapticClick ??= new RelayCommand(TestHaptic);

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
        
        private void TestHaptic(object commandParameter)
        {
            try
            {
                var app = (App)Application.Current;
                var hapticService = App.ServiceProvider.GetService<IVRHapticFeedbackService>();
                
                if (hapticService != null)
                {
                    MessageBox.Show("Running haptic test - check console for detailed logs", "Haptic Test", MessageBoxButton.OK, MessageBoxImage.Information);
                    hapticService.TestHapticFeedback();
                }
                else
                {
                    MessageBox.Show("VR Haptic Feedback Service not available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to test haptic feedback: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
