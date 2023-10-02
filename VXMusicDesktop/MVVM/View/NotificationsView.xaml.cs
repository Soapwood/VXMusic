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
using VXMusicDesktop.Theme;

namespace VXMusicDesktop.MVVM.View
{
    /// <summary>
    /// Interaction logic for NotificationsView.xaml
    /// </summary>
    public partial class NotificationsView : UserControl
    {
        public NotificationsView()
        {
            InitializeComponent();

            ColourSchemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            // Handle the theme change here for Home View
            NotificationsTextHeader.Foreground = ColourSchemeManager.TextBasic;

            NotificationService1TextHeader.Foreground = ColourSchemeManager.TextBasic;
            NotificationIntegration1BoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            NotificationIntegration1BoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;

            NotificationService2TextHeader.Foreground = ColourSchemeManager.TextBasic;
            NotificationIntegration2BoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            NotificationIntegration2BoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
        }
    }
}
