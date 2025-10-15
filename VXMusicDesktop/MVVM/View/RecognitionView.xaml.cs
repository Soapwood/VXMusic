using System;
using System.Windows;
using System.Windows.Controls;
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

            RunRecognitionButton.Background = ColourSchemeManager.SecondaryColour;
            RunRecognitionButton.Foreground = ColourSchemeManager.TextBasic;
        }
    }
}
