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
using VXMusicDesktop.Theme;

namespace VXMusicDesktop.MVVM.View
{
    /// <summary>
    /// Interaction logic for NotificationsView.xaml
    /// </summary>
    public partial class NotificationsView : UserControl
    {
        public NotificationsView()
        {
            InitializeComponent();

            ColourSchemeManager.ThemeChanged += OnThemeChanged;

            VXMusicSession.SteamVrNotificationEnabled += OnSteamVrNotificationEnabled;
            VXMusicSession.XsOverlayNotificationEnabled += OnXsOverlayNotificationEnabled;
            VXMusicSession.OvrToolkitNotificationEnabled += OnOvrToolkitNotificationEnabled;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            // Handle the theme change here for Home View
            NotificationsTextHeader.Foreground = ColourSchemeManager.TextBasic;
            
            if (NotificationService1Button.Resources["ButtonNormalGradientBrush"] is LinearGradientBrush normalGradientBrush1)
            {
                if (normalGradientBrush1.GradientStops.Count >= 2)
                {
                    normalGradientBrush1.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color;
                    normalGradientBrush1.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color;
                }
            }
            
            if (NotificationService1Button.Resources["ButtonHoverGradientBrush"] is LinearGradientBrush hoverGradientBrush1)
            {
                if (hoverGradientBrush1.GradientStops.Count >= 2)
                {
                    hoverGradientBrush1.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color; 
                    hoverGradientBrush1.GradientStops[1].Color = ColourSchemeManager.Accent2Colour.Color; 
                }
            }
            
            if (NotificationService2Button.Resources["ButtonNormalGradientBrush"] is LinearGradientBrush normalGradientBrush2)
            {
                if (normalGradientBrush2.GradientStops.Count >= 2)
                {
                    normalGradientBrush2.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color; 
                    normalGradientBrush2.GradientStops[1].Color = ColourSchemeManager.Accent1Colour.Color; 
                }
            }
            
            if (NotificationService2Button.Resources["ButtonHoverGradientBrush"] is LinearGradientBrush hoverGradientBrush2)
            {
                if (hoverGradientBrush2.GradientStops.Count >= 2)
                {
                    hoverGradientBrush2.GradientStops[0].Color = ColourSchemeManager.SecondaryColour.Color; 
                    hoverGradientBrush2.GradientStops[1].Color = ColourSchemeManager.Accent2Colour.Color;
                }
            }
            
            var textBlock1 = FindChildControl<TextBlock>(NotificationService1Button, "NotificationService1ButtonHeaderText");
            if (textBlock1 != null)
            {
                textBlock1.Foreground = new SolidColorBrush(ColourSchemeManager.TextBasic.Color); 
            }
            
            
            var textBlock2 = FindChildControl<TextBlock>(NotificationService2Button, "NotificationService2ButtonHeaderText");
            if (textBlock2 != null)
            {
                textBlock2.Foreground = new SolidColorBrush(ColourSchemeManager.TextBasic.Color); 
            }
        }
        protected virtual void OnSteamVrNotificationEnabled(object sender, EventArgs e)
        {
            //EnableSteamVRNotificationButton.Content = "Connected!";
            //EnableXsOverlayNotificationButton.Content = "Use";
        }

        protected virtual void OnXsOverlayNotificationEnabled(object sender, EventArgs e)
        {
            //EnableXsOverlayNotificationButton.Content = "Connected!";
            //EnableSteamVRNotificationButton.Content = "Use";
        }
        
        protected virtual void OnOvrToolkitNotificationEnabled(object sender, EventArgs e)
        {
            //EnableXsOverlayNotificationButton.Content = "Connected!";
            //EnableSteamVRNotificationButton.Content = "Use";
        }
        
        private T FindChildControl<T>(DependencyObject parent, string controlName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T && (child as FrameworkElement)?.Name == controlName)
                {
                    return (T)child;
                }
                else
                {
                    T result = FindChildControl<T>(child, controlName);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
    }
}
