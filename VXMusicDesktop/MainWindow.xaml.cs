using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VXMusicDesktop.Theme;
using VXMusicDesktop.Toast;
using VXMusicDesktop.Update;

namespace VXMusicDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;

            VersionTextBlock.Text = App.ApplicationVersion.ToString() + "-beta";

            ColourSchemeManager.ThemeChanged += OnThemeChanged;
            ColourSchemeManager.MenuOptionChanged += OnMainWindowMenuOptionChanged;
            
            // Anchor toast notifications to Main window instance
            App.VXMusicSession.ToastNotification = new ToastNotificationClient(this);
            
            CheckForUpdates();
            FocusMainWindow();
        }
        
        private void MainWindow_StateChanged(object? sender, EventArgs eventArgs)
        {
            App.VXMusicSession.ToastNotification.ShouldToastNotificationsBeShown = this.WindowState == WindowState.Minimized;
        }
        
        private void MainWindow_Activated(object? sender, EventArgs eventArgs)
        {
            App.VXMusicSession.ToastNotification.ShouldToastNotificationsBeShown = this.WindowState == WindowState.Normal;
        }
        
        private void MainWindow_Deactivated(object? sender, EventArgs eventArgs)
        {
            App.VXMusicSession.ToastNotification.ShouldToastNotificationsBeShown = this.WindowState == WindowState.Normal;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            // Handle the theme change here, e.g., update UI elements

            MainWindowBorder.Background = ColourSchemeManager.PrimaryColour;
            MainWindowTitleText.Foreground = ColourSchemeManager.TextBasic;

            MainWindowRadioButtonHome.Foreground = ColourSchemeManager.TextBasic;
            MainWindowRadioButtonRecognition.Foreground = ColourSchemeManager.TextBasic;
            MainWindowRadioButtonNotifications.Foreground = ColourSchemeManager.TextBasic;
            MainWindowRadioButtonConnections.Foreground = ColourSchemeManager.TextBasic;
            MainWindowRadioButtonOverlay.Foreground = ColourSchemeManager.TextBasic;
            MainWindowRadioButtonAbout.Foreground = ColourSchemeManager.TextBasic;

            MainWindowRadioButtonHome.Background = (bool)MainWindowRadioButtonHome.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
            MainWindowRadioButtonRecognition.Background = (bool)MainWindowRadioButtonRecognition.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
            MainWindowRadioButtonNotifications.Background = (bool)MainWindowRadioButtonNotifications.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
            MainWindowRadioButtonConnections.Background = (bool)MainWindowRadioButtonConnections.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
            MainWindowRadioButtonOverlay.Background = (bool)MainWindowRadioButtonOverlay.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
            MainWindowRadioButtonAbout.Background = (bool)MainWindowRadioButtonAbout.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;

            DesktopThemeComboBoxHintText.Foreground = ColourSchemeManager.TextBasic;
            DesktopThemeComboBoxHintText.Visibility = Visibility.Visible;
            DesktopThemeComboBox.Foreground = ColourSchemeManager.TextBasic;
            DesktopThemeComboBox.Background = ColourSchemeManager.SecondaryColour;
            DesktopThemeComboBox.BorderBrush = ColourSchemeManager.TextBasic;

            MainWindowMinimiseButtonImageBrush.ImageSource = ColourSchemeManager.MinimiseImage;
            MainWindowCloseButtonImageBrush.ImageSource = ColourSchemeManager.CloseImage;
        }

        private void OnMainWindowMenuOptionChanged(object sender, EventArgs e)
        {
            MainWindowRadioButtonHome.Background = (bool)MainWindowRadioButtonHome.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
            MainWindowRadioButtonRecognition.Background = (bool)MainWindowRadioButtonRecognition.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
            MainWindowRadioButtonNotifications.Background = (bool)MainWindowRadioButtonNotifications.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
            MainWindowRadioButtonConnections.Background = (bool)MainWindowRadioButtonConnections.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
            MainWindowRadioButtonOverlay.Background = (bool)MainWindowRadioButtonOverlay.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
            MainWindowRadioButtonAbout.Background = (bool)MainWindowRadioButtonAbout.IsChecked ? ColourSchemeManager.SecondaryColour : ColourSchemeManager.PrimaryColour;
        }
        
        public void FocusMainWindow()
        {
            this.WindowState = WindowState.Normal;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ThemeDropdownSelection(object sender, SelectionChangedEventArgs e)
        {
            // Show Hint Text above combo box if Dropdown isn't open
            CheckThemeComboBoxVisibility();

            if (sender is ComboBox comboBox)
            {
                if (comboBox.SelectedIndex >= 0)
                {
                    string selectedOption = comboBox.SelectedItem.ToString();
                    DesktopTheme themeToUpdateTo = (DesktopTheme) Enum.Parse(typeof(DesktopTheme), selectedOption);

                    VXUserSettings.Desktop.SetDesktopThemeInSettings(themeToUpdateTo);
                    ColourSchemeManager.SetTheme(themeToUpdateTo);
                    ColourSchemeManager.RaiseThemeChanged();
                }
            }
        }

        private void CheckThemeComboBoxVisibility()
        {
            // Show Hint Text above combo box if Dropdown isn't open
            if (DesktopThemeComboBox.IsDropDownOpen)
            {
                DesktopThemeComboBoxHintText.Visibility = Visibility.Collapsed;
            }
            else
            {
                DesktopThemeComboBoxHintText.Visibility = DesktopThemeComboBox.SelectedItem == null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SetMessageTextBox(string messageContent)
        {
            MessageTextBlock.Text = messageContent;
        }
        
        async void CheckForUpdates()
        {
            bool isUpdateAvailable = await App.VXMusicSession.VxMusicUpdate.IsVxMusicUpdateAvailable();

            if (isUpdateAvailable)
            {
                SetMessageTextBox("An update is available!");

                if (VXUserSettings.Settings.GetAskForUpdatesOnStartup())
                {
                    var updateRequested = MessageBox.Show($"An update for VXMusic is available!{Environment.NewLine}Would you like to update now?", "VXMusic Update Available", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
                    if (updateRequested == MessageBoxResult.Yes)
                        App.VXMusicSession.VxMusicUpdate.LaunchVxMusicUpdater();
                    else
                        FocusMainWindow();
                }
            }
        }
    }
}
