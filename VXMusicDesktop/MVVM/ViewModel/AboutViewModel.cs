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

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class AboutViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private RelayCommand askForUpdatesToggleButton;

        public ICommand AskForUpdatesOnStartupToggleButton =>
            askForUpdatesToggleButton ??= new RelayCommand(SetAskForUpdatesOnStartup);
        
        private bool _askForUpdatesOnStartup;

        public AboutViewModel()
        {
            AskForUpdatesOnStartup = VXUserSettings.Settings.GetAskForUpdatesOnStartup();
        }
        
        public bool AskForUpdatesOnStartup
        {
            get { return _askForUpdatesOnStartup; }
            set
            {
                if (_askForUpdatesOnStartup != value)
                {
                    _askForUpdatesOnStartup = value;
                    OnPropertyChanged(nameof(AskForUpdatesOnStartup));
                }
            }
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public void SetAskForUpdatesOnStartup(object commandParameter)
        {
            VXUserSettings.Settings.SetAskForUpdatesOnStartup(_askForUpdatesOnStartup);
        }
        
    }
}
