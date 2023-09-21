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
    internal class AboutViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        //private RelayCommand _openUrlInBrowserClick;

        //public ICommand OpenUrlInBrowserClick => _openUrlInBrowserClick ??= new RelayCommand(PerformOpenUrlInBrowserClick);

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //private void PerformOpenUrlInBrowserClick(object commandParameter)
        //{
        //    var link = (Hyperlink)commandParameter;

        //    Process.Start(new ProcessStartInfo
        //    {
        //        FileName = link.NavigateUri.ToString(),
        //        UseShellExecute = true
        //    });
        //}
    }
}
