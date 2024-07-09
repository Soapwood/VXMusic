using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using Octokit;
using VXAutoUpdaterDesktop.Core;

namespace VXAutoUpdaterDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VXMusicAutoUpdater _autoUpdater;

        private IReadOnlyList<Release> _availableReleases;

        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;

            string personalAccesstoken =
                "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";
            _autoUpdater = new VXMusicAutoUpdater("Soapwood", "VXMusic", personalAccesstoken);
            GetLatestSupportedVersions();
            LoadComboBoxItems();

            UpdateTextBlockMessage("");

            FocusMainWindow();
        }
        
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.StopRunningVXMusicDesktopProcesses();
            
            VXMusicAutoUpdater.UpdateApplicationBasedOnRequestedVersion(VXMusicAutoUpdater.ReleaseBranchOptions[BranchComboBox.SelectedIndex],
                _availableReleases[VersionComboBox.SelectedIndex],
                _autoUpdater);
        }
        
        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchUpdatedVXMusicDesktop();
        }
        
        public void UpdateTextBlockMessage(string message)
        {
            if (MessageTextBlock != null)
            {
                MessageTextBlock.Text = message;
            }
        }
        
        public void UpdateProgressBar(long progressPercent)
        {
            if (ReleaseUpdateProgressBar != null)
            {
                ReleaseUpdateProgressBar.Value = progressPercent;
            }
        }
        
        public void FocusMainWindow()
        {
            this.WindowState = WindowState.Normal;
        }
        
        private void LoadComboBoxItems()
        {
            BranchComboBox.ItemsSource = VXMusicAutoUpdater.ReleaseBranchOptions;
            BranchComboBox.SelectedIndex = 0;
            BranchComboBox.SelectedItem = "Stable";
        }

        private async Task GetLatestSupportedVersions()
        {
            _availableReleases = await _autoUpdater.GetLatestVersionsForBranch("Stable");
            VersionComboBox.ItemsSource = _availableReleases.Select(e => e.TagName);
            VersionComboBox.SelectedIndex = 0;
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
        
        private void LaunchUpdatedVXMusicDesktop()
        {
            UpdateTextBlockMessage("Launching VXMusic...");
            
            try
            {
                // Define the registry path and key
                string registryPath = @"SOFTWARE\WOW6432Node\VirtualXtensions\VXMusic"; // TODO this is bound to 32bit
                string installPathKey = "InstallPath";
                string executableKey = "Executable";

                // Read the installation path from the registry
                string installPath = ReadRegistryValue(RegistryHive.LocalMachine, registryPath, installPathKey);
                string executablePath = ReadRegistryValue(RegistryHive.LocalMachine, registryPath, executableKey);

                if (string.IsNullOrEmpty(installPath) || string.IsNullOrEmpty(executablePath))
                {
                    MessageBox.Show("The installation path or executable path could not be found in the registry.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!File.Exists(executablePath))
                {
                    MessageBox.Show($"Executable not found: {executablePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Start the process
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    UseShellExecute = true, // Enable shell execute
                    WorkingDirectory = Path.GetDirectoryName(executablePath), // Set the working directory
                    RedirectStandardOutput = false, // Do not redirect output
                    RedirectStandardError = false, // Do not redirect error
                    CreateNoWindow = false, // Create window
                };

                Process process = Process.Start(startInfo);

                if (process != null)
                {
                    UpdateTextBlockMessage("Launching VXMusic...");
                    System.Threading.Thread.Sleep(2000); // Optional: Wait for a moment to ensure the process starts
                    Environment.Exit(0); // Exit the current application
                }
                else
                {
                    MessageBox.Show($"Failed to start process for {executablePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start new application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ReadRegistryValue(RegistryHive hive, string subKey, string keyName)
        {
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64))
            {
                using (RegistryKey key = baseKey.OpenSubKey(subKey))
                {
                    return key?.GetValue(keyName)?.ToString();
                }
            }
        }
    }
}