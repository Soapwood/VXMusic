﻿using com.csutil;
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

        public readonly string UsernameBoxHintText = "Username";
        public readonly string PasswordBoxHintText = "Password";

        public ConnectionsView()
        {
            InitializeComponent();

            CheckLastFmConnection();

            ColourSchemeManager.ThemeChanged += OnThemeChanged;
            VXMusicSession.LastFmLogin += OnLastFmLogin;
            SpotifyAuthentication.SpotifyLogin += OnSpotifyLogin;
        }

        protected virtual void OnLastFmLogin(object sender, EventArgs e)
        {
            LastFmLoginButton.Content = "Connected!";
        }

        protected virtual void OnSpotifyLogin(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => // OMFG THIS WORKS TO UPDATE THE UI ALL THE TIME
            {
                SpotifyLoginButton.Content = SpotifyConnectionStateExtensions.ToDisplayString(SpotifyAuthentication.CurrentConnectionState);
            });
        }

        private void CheckLastFmConnection()
        {
            //LastFmUsernameTextBox.Text = App.VXMusicSession.ConnectionsSettings.LastfmSettings.Username;
            //Box.Password = "*****************";

            LastFmUsernameBoxHintText.Visibility = Visibility.Hidden;
            LastFmPasswordBoxHintText.Visibility = Visibility.Hidden;
            LastFmLoginButton.Content = "Connected!";
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            // Handle the theme change here for Home View
            ConnectionsTextHeader.Foreground = ColourSchemeManager.TextBasic;

            ConnectionService1TextHeader.Foreground = ColourSchemeManager.TextBasic;
            ConnectionService1TextParagraph.Foreground = ColourSchemeManager.TextBasic;
            ConnectionsIntegration1BoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            ConnectionsIntegration1BoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;

            ConnectionService2TextHeader.Foreground = ColourSchemeManager.TextBasic;
            ConnectionService2TextParagraph.Foreground = ColourSchemeManager.TextBasic;
            ConnectionsIntegration2BoxBorderGradientBrush.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
            ConnectionsIntegration2BoxBorderGradientBrush.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
        }

        private void LastFmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ConnectionsViewModel connectionsViewModel)
            {
                connectionsViewModel.SharedViewModel.IsLastFmConnected = false;
                connectionsViewModel.LastFmPassword = ((PasswordBox)e.OriginalSource).Password;

                LastFmPasswordBoxHintText.Visibility = String.IsNullOrEmpty(connectionsViewModel.LastFmPassword) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void LastFmPasswordTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LastFmPasswordBoxHintText.Visibility = Visibility.Hidden;
        }

        private void LastFmPasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ConnectionsViewModel connectionsViewModel)
            {
                connectionsViewModel.LastFmPassword = ((PasswordBox)e.OriginalSource).Password;
                LastFmPasswordBoxHintText.Visibility = String.IsNullOrEmpty(connectionsViewModel.LastFmPassword) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void LastFmUsernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.DataContext is ConnectionsViewModel connectionsViewModel)
            {
                connectionsViewModel.SharedViewModel.IsLastFmConnected = false;
                connectionsViewModel.LastFmUsername = ((TextBox) e.OriginalSource).Text;
            }
        }

        private void LastFmUsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ConnectionsViewModel connectionsViewModel)
            {
                LastFmUsernameBoxHintText.Visibility = String.IsNullOrEmpty(connectionsViewModel.LastFmUsername) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void LastFmUsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (this.DataContext is ConnectionsViewModel connectionsViewModel)
            {
                if (connectionsViewModel.LastFmUsername != null)
                {
                    textBox.Text = connectionsViewModel.LastFmUsername;
                }
                else
                {
                    textBox.Text = UsernameBoxHintText;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SpotifyLoginButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
