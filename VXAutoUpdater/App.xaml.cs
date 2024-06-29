using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using VXAutoUpdaterDesktop.Theme;

namespace VXAutoUpdaterDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            if (IsAdministrator())
            {
                ColourSchemeManager.SetTheme();
                //UpdateAppIfNeeded().Wait();
            }
            else
            {
                RestartAsAdministrator();
            }
        }
        
        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    
        private static void RestartAsAdministrator()
        {
            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            var startInfo = new ProcessStartInfo(exeName)
            {
                UseShellExecute = true,
                Verb = "runas"
            };
            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("This operation requires administrator privileges. Please restart the application as an administrator.", "Administrator Privileges Required");
            }
            Environment.Exit(0);
        }
    }
}