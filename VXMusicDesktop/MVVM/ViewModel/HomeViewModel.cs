using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusic.Connections.Http;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();
        public event PropertyChangedEventHandler? PropertyChanged;
        public HomeViewModel(SharedViewModel sharedViewModel)
        {
            SharedViewModel = sharedViewModel;
            FetchVxNewsText();
        }
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        // Shared ViewModel for sharing concurrency values between certain Views.
        public SharedViewModel SharedViewModel { get; }
        private string _vxNewsGistText { get; set; }
        
        public string VxNewsGistText
        {
            get { return _vxNewsGistText; }
            set
            {
                if (_vxNewsGistText != value)
                {
                    _vxNewsGistText = value;
                    OnPropertyChanged(nameof(VxNewsGistText));
                }
            }
        }

        public async Task FetchVxNewsText()
        {
            Logger.LogDebug("Fetching VXNews from Github Gist");
            VxNewsGistText = await VxHttpClient.FetchVxNewsGistContent();
            Logger.LogDebug($"Received: {VxNewsGistText}");
        }
    }
}
