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
        public static VXMusicSession VXMusicSession;

        public App()
        {
            Trace.WriteLine("Booting VXMusic");
            VXMusicSession = new VXMusicSession();
            VXMusicSession.InitialiseVXMusicSession();

            //Properties.Settings.Default.Save();
        }
    }
}
