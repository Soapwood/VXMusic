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
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            ColourSchemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            // Handle the theme change here for Home View
            HomeTextHeader.Foreground = ColourSchemeManager.TextBasic;

            MainBoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            MainBoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;

            MainBoxBorderHeaderText.Foreground = ColourSchemeManager.TextBasic;
            MainBoxBorderParagraphText.Foreground = ColourSchemeManager.TextBasic;

            ReleaseNotesBoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            ReleaseNotesBoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;

            ReleaseNotesBoxBorderHeaderText.Foreground = ColourSchemeManager.TextBasic;
            VxMusicNewsBoxParagraphText.Foreground = ColourSchemeManager.TextBasic;

            IntegrationsHeaderText.Foreground = ColourSchemeManager.TextBasic;

            RecognitionIntegrationsBoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            RecognitionIntegrationsBoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
            RecognitionIntegrationsBoxHeaderText.Foreground = ColourSchemeManager.TextBasic;
            API1RecognitionIntegrationsBoxHeaderText.Foreground = ColourSchemeManager.TextBasic;
            API2RecognitionIntegrationsBoxHeaderText.Foreground = ColourSchemeManager.TextBasic;

            NotificationIntegrationsBoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            NotificationIntegrationsBoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
            NotificationIntegrationsBoxHeaderText.Foreground = ColourSchemeManager.TextBasic;
            Service1NotificationIntegrationsBoxHeaderText.Foreground = ColourSchemeManager.TextBasic;
            Service2NotificationIntegrationsBoxHeaderText.Foreground = ColourSchemeManager.TextBasic;

            ConnectionIntegrationsBoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            ConnectionIntegrationsBoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
            ConnectionIntegrationsBoxHeaderText.Foreground = ColourSchemeManager.TextBasic;
            Connection1NotificationIntegrationsBoxHeaderText.Foreground = ColourSchemeManager.TextBasic;
            Connection2NotificationIntegrationsBoxHeaderText.Foreground = ColourSchemeManager.TextBasic;

            GameClientIntegrationsBoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            GameClientIntegrationsBoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
            GameClientIntegrationsBoxHeaderText.Foreground = ColourSchemeManager.TextBasic;
        }
    }
}
