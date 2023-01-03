using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Config.Net;

namespace Ruler
{
    public static class RulerSettings
    {
        private static readonly IRulerSettings settings;

        public static IRulerSettings Default => settings;

        public static ILayout CurrentLayout => Default.Orientation == Orientation.Horizontal ?
            Default.HorizontalRuler : Default.VerticalRuler;

        public static IColorPalette CurrentTheme => Default.ColorTheme == Theme.Light ? LightTheme :
            Default.ColorTheme == Theme.Dark ? DarkTheme : Default.CustomColors;

        static RulerSettings()
        {
            settings = new ConfigurationBuilder<IRulerSettings>()
                .UseTypeParser(new ColorParser())
                .UseJsonFile("settings.json")
                .Build();


            if (settings.CustomColors.Foreground == default)
            {
                settings.CustomColors.Foreground = Brushes.Black;
                settings.CustomColors.Background = Brushes.White;
                settings.CustomColors.Marker = Brushes.ForestGreen;
                settings.CustomColors.Angle = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF6600"));
                settings.CustomColors.Info = SystemColors.InfoBrush;
            }

            if (settings.HorizontalRuler.Width < 0)
            {
                settings.HorizontalRuler.Left = 200;
                settings.HorizontalRuler.Top = 400;
                settings.HorizontalRuler.Width = 800;
                settings.HorizontalRuler.Height = 80;
            }

            if (settings.VerticalRuler.Height < 0)
            {
                settings.VerticalRuler.Left = 200;
                settings.VerticalRuler.Top = 200;
                settings.VerticalRuler.Width = 80;
                settings.VerticalRuler.Height = 600;
            }
        }

        public static IColorPalette LightTheme => new ColorPalette();

        public static IColorPalette DarkTheme => new ColorPalette()
        {
            Foreground = Brushes.White,
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF202020")),
            Mouse = Brushes.LightGreen,
            Marker = Brushes.Violet,
            Angle = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF6600")),
            Info = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF606060")),
        };


    }

    public class ColorPalette : IColorPalette
    {
        public ColorPalette()
        {
            Foreground = Brushes.Black;
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFAFAFA"));
            Mouse = Brushes.ForestGreen;
            Marker = Brushes.DarkMagenta;
            Angle = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF6600"));
            Info = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
        }

        public SolidColorBrush Foreground { get; set; }
        public SolidColorBrush Background { get; set; }
        public SolidColorBrush Mouse { get; set; }
        public SolidColorBrush Marker { get; set; }
        public SolidColorBrush Angle { get; set; }
        public SolidColorBrush Info { get; set; }
    }
}
