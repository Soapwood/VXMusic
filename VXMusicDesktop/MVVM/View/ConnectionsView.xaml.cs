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
using VXMusic.Spotify.Authentication;
using VXMusicDesktop.MVVM.ViewModel;
using VXMusicDesktop.Theme;

namespace VXMusicDesktop.MVVM.View
{
    /// <summary>
    /// Interaction logic for ConnectionsView.xaml
    /// </summary>
    public partial class ConnectionsView : UserControl
    {
        public bool IsSpotifyLoggedIn;

        public ConnectionsView()
        {
            InitializeComponent();

            ColourSchemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            // Handle the theme change here for Home View
            ConnectionsTextHeader.Foreground = ColourSchemeManager.TextBasic;

            ConnectionService1TextHeader.Foreground = ColourSchemeManager.TextBasic;
            ConnectionService1TextParagraph.Foreground = ColourSchemeManager.TextBasic;

            ConnectionService2TextHeader.Foreground = ColourSchemeManager.TextBasic;
            ConnectionService2TextParagraph.Foreground = ColourSchemeManager.TextBasic;
        }
    }
}
