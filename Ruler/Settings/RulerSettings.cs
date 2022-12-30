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

        static RulerSettings()
        {
            settings = new ConfigurationBuilder<IRulerSettings>()
                .UseTypeParser(new ColorParser())
                .UseJsonFile("settings.json")
                .Build();


            if (settings.Foreground == default)
            {
                settings.Foreground = Brushes.Black;
            }
            if (settings.Background == default)
            {
                settings.Background = Brushes.White;
            }
            if (settings.MarkerBrush == default)
            {
                settings.MarkerBrush = Brushes.ForestGreen;
            }
            if (settings.AngleBrush == default)
            {
                settings.AngleBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF6600"));
            }
            if (settings.InfoBrush == default)
            {
                settings.InfoBrush = SystemColors.InfoBrush;
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

    }
}
