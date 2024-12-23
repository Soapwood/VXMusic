﻿using System;
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
        private readonly string ShazamApiKeyPlaceholder = "•••••••••••••••••••••••••••••••";
        private readonly string AudDApiKeyPlaceholder = "•••••••••••••••••••••••••••••••";
        
        public RecognitionView()
        {
            InitializeComponent();
            
            // TODO Populate BYOAPI PasswordBox if BYOAPI is enabled and value is in Settings.
            
            ColourSchemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            // Handle the theme change here for Home View
            RecognitionTextHeader.Foreground = ColourSchemeManager.TextBasic;

            //RecognitionIntegration1BoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            //RecognitionIntegration1BoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
            //RecognitionIntegration1HeaderText.Foreground = ColourSchemeManager.TextBasic;

            //RecognitionIntegration2BoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            //RecognitionIntegration2BoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
            //RecognitionIntegration2HeaderText.Foreground = ColourSchemeManager.TextBasic;

            RunRecognitionButton.Background = ColourSchemeManager.SecondaryColour;
            RunRecognitionButton.Foreground = ColourSchemeManager.TextBasic;

            //ShazamByoApiCheckBox.Background = ColourSchemeManager.Accent1Colour;
            //ShazamByoApiCheckBox.Foreground = ColourSchemeManager.TextBasic;
            ShazamByoApiPasswordBox.Background = ColourSchemeManager.SecondaryColour;
            
            //AudDByoApiCheckBox.Background = ColourSchemeManager.Accent1Colour;
            //AudDByoApiCheckBox.Foreground = ColourSchemeManager.TextBasic;
            AudDByoApiPasswordBox.Background = ColourSchemeManager.SecondaryColour;

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
        
        private void ShazamByoApiCheckButtonChecked(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RecognitionViewModel recognitionViewModel)
            {
                if (recognitionViewModel.IsAudDApiEnabled)
                    return;
                
                recognitionViewModel.SharedViewModel.IsShazamApiConnected = false;
                recognitionViewModel.IsShazamByoApiEnabled = true;

                //ShazamByoApiCheckBox.Content = "Disable BYOAPI";
                //ShazamByoApiCheckBox.Content = "";
                if(!string.IsNullOrEmpty(recognitionViewModel.ShazamByoApiToken))
                    ShazamByoApiPasswordBoxHintText.Text = ShazamApiKeyPlaceholder;

                recognitionViewModel.PerformSaveAndTestShazamByoApi();
                //LastFmPasswordBoxHintText.Visibility = String.IsNullOrEmpty(connectionsViewModel.LastFmPassword) ? Visibility.Visible : Visibility.Hidden;
            }
        }
        
        private void ShazamByoApiCheckButtonUnchecked(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RecognitionViewModel recognitionViewModel)
            {
                recognitionViewModel.SharedViewModel.IsShazamApiConnected = false;
                recognitionViewModel.IsShazamByoApiEnabled = false;
                
                //ShazamByoApiCheckBox.Content = "Enable BYOAPI";
                ShazamByoApiPasswordBoxHintText.Text = "";
    
                if(recognitionViewModel.IsShazamApiEnabled)
                    recognitionViewModel.SetShazamApiKeyToDefaultAndTest();
            }
        }
        
        
        private void ShazamByoApiPasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RecognitionViewModel recognitionViewModel)
            {
                ShazamByoApiPasswordBoxHintText.Text = "";
                recognitionViewModel.SharedViewModel.IsShazamApiConnected = false;
                recognitionViewModel.ShazamByoApiToken = ((PasswordBox)e.OriginalSource).Password;
                
                recognitionViewModel.PerformSaveAndTestShazamByoApi();
            }
        }

        private void ShazamByoApiPasswordBoxGotFocus(object sender, RoutedEventArgs e)
        {
            ShazamByoApiPasswordBoxHintText.Text = "";
        }

        private void ShazamByoApiPasswordBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RecognitionViewModel recognitionViewModel)
            {
                if(string.IsNullOrEmpty(recognitionViewModel.ShazamByoApiToken))
                    ShazamByoApiPasswordBoxHintText.Text = ShazamApiKeyPlaceholder;
                recognitionViewModel.ShazamByoApiToken = ((PasswordBox)e.OriginalSource).Password;
            }
        }
        
        private void PerformSaveAndTestShazamByoApi(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RecognitionViewModel recognitionViewModel)
            {
                recognitionViewModel.PerformSaveAndTestShazamByoApi();
            }
        }
        
        private void AudDByoApiPasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RecognitionViewModel recognitionViewModel)
            {
                AudDByoApiPasswordBoxHintText.Text = "";
                recognitionViewModel.SharedViewModel.IsAudDApiConnected = false;
                recognitionViewModel.AudDByoApiToken = ((PasswordBox)e.OriginalSource).Password;
                
                recognitionViewModel.PerformSaveAndTestAudDByoApi();
            }
        }

        private void AudDByoApiPasswordBoxGotFocus(object sender, RoutedEventArgs e)
        {
            AudDByoApiPasswordBoxHintText.Text = "";
        }

        private void AudDByoApiPasswordBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RecognitionViewModel recognitionViewModel)
            {
                if(string.IsNullOrEmpty(recognitionViewModel.AudDByoApiToken))
                    AudDByoApiPasswordBoxHintText.Text = AudDApiKeyPlaceholder;
                recognitionViewModel.AudDByoApiToken = ((PasswordBox)e.OriginalSource).Password;
            }
        }

        private void AudDByoApiCheckButtonChecked(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RecognitionViewModel recognitionViewModel)
            {
                if(!string.IsNullOrEmpty(recognitionViewModel.AudDByoApiToken))
                    AudDByoApiPasswordBoxHintText.Text = AudDApiKeyPlaceholder;
                
                recognitionViewModel.SharedViewModel.IsAudDApiConnected = false;
                recognitionViewModel.IsAudDByoApiEnabled = true;

                //AudDByoApiCheckBox.Content = "Disable BYOAPI";
                //AudDByoApiCheckBox.Content = "";

                recognitionViewModel.PerformSaveAndTestAudDByoApi();
            }
        }
        
        private void AudDByoApiCheckButtonUnchecked(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RecognitionViewModel recognitionViewModel)
            {
                AudDByoApiPasswordBoxHintText.Text = "";
                
                recognitionViewModel.SharedViewModel.IsAudDApiConnected = false;
                recognitionViewModel.IsAudDByoApiEnabled = false;
                
                //AudDByoApiCheckBox.Content = "Enable BYOAPI";

                if(recognitionViewModel.IsAudDApiEnabled)
                    recognitionViewModel.SetAudDApiKeyToDefaultAndTest();
            }
        }
        
        private void PerformSaveAndTestAudDByoApi(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RecognitionViewModel recognitionViewModel)
            {
                recognitionViewModel.PerformSaveAndTestAudDByoApi();
            }
        }
    }
}
