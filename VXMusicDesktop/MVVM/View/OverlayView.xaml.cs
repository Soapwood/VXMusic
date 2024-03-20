using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VXMusicDesktop.MVVM.View
{
    /// <summary>
    /// Interaction logic for OverlayView.xaml
    /// </summary>
    public partial class OverlayView : UserControl
    {
        public OverlayView()
        {
            InitializeComponent();
        }
        
        private void SetLaunchOverlayOnStartupEnabled(object commandParameter, RoutedEventArgs routedEventArgs)
        {
            App.VXMusicSession.SetLaunchOverlayOnStartup(true);
        }

        private void SetLaunchOverlayOnStartupDisabled(object commandParameter, RoutedEventArgs routedEventArgs)
        {
            App.VXMusicSession.SetLaunchOverlayOnStartup(false);
        }
    }
}
