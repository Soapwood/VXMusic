using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VXMusic;

namespace VXMusicDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static VXMusicSession Session;

        public App()
        {
            Trace.WriteLine("Booting VXMusic");
            Session = new VXMusicSession();
            Session.InitialiseVXMusicSession();

            //Properties.Settings.Default.Save();
        }
    }
}
