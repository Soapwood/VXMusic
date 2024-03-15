using System;
using System.Collections.Generic;
using System.Globalization;
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
using VXMusicDesktop.MVVM.ViewModel;
using VXMusicDesktop.Theme;

namespace VXMusicDesktop.MVVM.View
{
    /// <summary>
    /// Interaction logic for RecognitionView.xaml
    /// </summary>
    public partial class RecognitionView : UserControl
    {
        public RecognitionView()
        {
            InitializeComponent();

            ColourSchemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            // Handle the theme change here for Home View
            RecognitionTextHeader.Foreground = ColourSchemeManager.TextBasic;

            RecognitionIntegration1BoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            RecognitionIntegration1BoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
            RecognitionIntegration1HeaderText.Foreground = ColourSchemeManager.TextBasic;

            RecognitionIntegration2BoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            RecognitionIntegration2BoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
            RecognitionIntegration2HeaderText.Foreground = ColourSchemeManager.TextBasic;

            RunRecognitionButton.Background = ColourSchemeManager.SecondaryColour;
            RunRecognitionButton.Foreground = ColourSchemeManager.TextBasic;

            // if (RecognitionIntegrationEnableShazamApiButton.DataContext as RecognitionViewModel)
            //     RecognitionIntegrationEnableShazamApiButton.Background = ColourSchemeManager.SecondaryColour;
            //
            // if (RecognitionIntegrationEnableAudDApiButton.DataContext.ToString() == "Active")
            //     RecognitionIntegrationEnableAudDApiButton.Background = ColourSchemeManager.SecondaryColour;

            //EnableShazamBYOAPIRadioButton.Background = ColourSchemeManager.Accent2Colour;
            //EnableShazamBYOAPIRadioButton.Opacity = 0;
            //EnableAudDBYOAPIRadioButton.Background= ColourSchemeManager.Accent2Colour;
            //EnableAudDBYOAPIRadioButton.Opacity = 0;
        }

        private void EnableShazamBYOAPIRadioButton_Click(object sender, RoutedEventArgs e)
        {
            //EnableShazamBYOAPIRadioButton.IsChecked = !EnableShazamBYOAPIRadioButton.IsChecked;
        }
        
        private void EnableAudDBYOAPIRadioButton_Click(object sender, RoutedEventArgs e)
        {
            //EnableAudDBYOAPIRadioButton.IsChecked = !EnableAudDBYOAPIRadioButton.IsChecked;
        }
    }
}
