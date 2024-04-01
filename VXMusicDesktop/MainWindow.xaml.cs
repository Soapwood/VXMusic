using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VXMusicDesktop.Theme;

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

            VersionTextBlock.Text = App.ApplicationVersion.ToString();

            ColourSchemeManager.ThemeChanged += OnThemeChanged;
            ColourSchemeManager.MenuOptionChanged += OnMainWindowMenuOptionChanged;
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
            this.Close(); // TODO Close down services gracfully
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
    }
}
