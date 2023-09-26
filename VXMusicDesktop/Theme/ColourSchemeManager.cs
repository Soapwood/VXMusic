using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace VXMusicDesktop.Theme
{
    public enum DesktopTheme
    {
        Darkmode1,
        Darkmode2,
        Lightmode1
    }

    public static class DesktopThemeExtensions
    {
        public static DesktopTheme[] Values
        {
            get
            {
                return Enum.GetValues(typeof(DesktopTheme))
                           .Cast<DesktopTheme>()
                           .ToArray();
            }
        }
    }

    public class ColourSchemeManager
    {
        public static event EventHandler ThemeChanged;

        public static readonly string Darkmode1Primary = "#252525";
        public static readonly string Darkmode1Secondary = "#6930C3";
        public static readonly string Darkmode1Accent1Colour = "#64DFDF";
        public static readonly string Darkmode1Accent2Colour = "#80FFDB";
        public static readonly string Darkmode1TextBasic = "#F8F0E3";

        public static readonly string Darkmode2Primary = "#000000";
        public static readonly string Darkmode2Secondary = "#52057B";
        public static readonly string Darkmode2Accent1Colour = "#892CDC";
        public static readonly string Darkmode2Accent2Colour = "#BC6FF1";
        public static readonly string Darkmode2TextBasic = "#F8F0E3";

        public static readonly string Lightmode1Primary = "#E4F1FF";
        public static readonly string Lightmode1Secondary = "#9400FF";
        public static readonly string Lightmode1Accent1Colour = "#AED2FF";
        public static readonly string Lightmode1Accent2Colour = "#27005D";
        public static readonly string Lightmode1TextBasic = "#040D12";

        public static readonly string SpotifyColour = "#1DB954";
        public static readonly string LastFmColour = "#C3000D";

        public static SolidColorBrush PrimaryColour { get; set; }
        public static SolidColorBrush SecondaryColour { get; set; }
        public static SolidColorBrush Accent1Colour { get; set; }
        public static SolidColorBrush Accent2Colour { get; set; }
        public static SolidColorBrush TextBasic { get; set; }
        public static SolidColorBrush SpotifyGreen { get; set; } = new SolidColorBrush(FromHex(SpotifyColour));
        public static SolidColorBrush LastFmRed { get; set; } = new SolidColorBrush(FromHex(LastFmColour));

        // This is used to access the values of the DesktopTheme Enum in XAML
        public static DesktopTheme[] DesktopThemeValues
        {
            get
            {
                return Enum.GetValues(typeof(DesktopTheme))
                           .Cast<DesktopTheme>()
                           .ToArray();
            }
        }

        public static void SetTheme(DesktopTheme theme)
        {
            switch(theme)
            {
                case DesktopTheme.Darkmode1:
                    SetThemeDarkmode1();
                    break;
                case DesktopTheme.Darkmode2:
                    SetThemeDarkmode2();
                    break;
                case DesktopTheme.Lightmode1:
                    SetThemeLightmode1();
                    break;
            }
        }
        public static void RaiseThemeChanged()
        {
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void SetThemeDarkmode1()
        {
            PrimaryColour = new SolidColorBrush(FromHex(Darkmode1Primary));
            SecondaryColour = new SolidColorBrush(FromHex(Darkmode1Secondary));
            Accent1Colour = new SolidColorBrush(FromHex(Darkmode1Accent1Colour));
            Accent2Colour = new SolidColorBrush(FromHex(Darkmode1Accent2Colour));
            TextBasic = new SolidColorBrush(FromHex(Darkmode1TextBasic));
        }

        private static void SetThemeDarkmode2()
        {
            PrimaryColour = new SolidColorBrush(FromHex(Darkmode2Primary));
            SecondaryColour = new SolidColorBrush(FromHex(Darkmode2Secondary));
            Accent1Colour = new SolidColorBrush(FromHex(Darkmode2Accent1Colour));
            Accent2Colour = new SolidColorBrush(FromHex(Darkmode2Accent2Colour));
            TextBasic = new SolidColorBrush(FromHex(Darkmode2TextBasic));
        }

        private static void SetThemeLightmode1()
        {
            PrimaryColour = new SolidColorBrush(FromHex(Lightmode1Primary));
            SecondaryColour = new SolidColorBrush(FromHex(Lightmode1Secondary));
            Accent1Colour = new SolidColorBrush(FromHex(Lightmode1Accent1Colour));
            Accent2Colour = new SolidColorBrush(FromHex(Lightmode1Accent2Colour));
            TextBasic = new SolidColorBrush(FromHex(Lightmode1TextBasic));
        }

        private static Color FromHex(string hex)
        {
            hex = hex.TrimStart('#'); // Remove '#' if present
            if (hex.Length != 6)
                throw new ArgumentException("Hex color code must be 6 characters long.", nameof(hex));

            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);

            return Color.FromRgb(r, g, b);
        }
    }

    public class SolidColorBrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush solidColorBrush)
            {
                return solidColorBrush.Color;
            }
            return Colors.Transparent; // Return a default color if conversion fails.
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
