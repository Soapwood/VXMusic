﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
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

namespace VXMusicDesktop.MVVM.View
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void Url_Click(object sender, RoutedEventArgs e)
        {
            var link = (Hyperlink) sender;

            Process.Start(new ProcessStartInfo
            {
                FileName = link.NavigateUri.ToString(),
                UseShellExecute = true
            });
        }
    }
}
