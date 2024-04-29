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
    /// Interaction logic for OverlayView.xaml
    /// </summary>
    public partial class OverlayView : UserControl
    {
        public OverlayView()
        {
            ColourSchemeManager.ThemeChanged += OnThemeChanged;
            
            InitializeComponent();
        }
        
        private void OnThemeChanged(object sender, EventArgs e)
        {
            // Handle the theme change here for Home View
            OverlayMenuOption2BoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            OverlayMenuOption2BoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
            
            OverlayMenuOptionOverlayHandBoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            OverlayMenuOptionOverlayHandBoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;

            LaunchOverlayOnStartupToggleButton.BorderBrush = LaunchOverlayOnStartupToggleButton.IsChecked == true
                ? ColourSchemeManager.SecondaryColour
                : ColourSchemeManager.TextBasic;
        }
    }
}
