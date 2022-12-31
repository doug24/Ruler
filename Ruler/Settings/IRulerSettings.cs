using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruler
{
    public enum Theme { Light, Dark, Custom }

    public interface IRulerSettings
    {
        [DefaultValue(Orientation.Horizontal)]
        Orientation Orientation { get; set; }

        [DefaultValue(Units.DIP)]
        Units ScaleUnits { get; set; }

        [DefaultValue(1.6f)]
        float Magnification { get; set; }

        [DefaultValue(true)]
        bool Perpendicular { get; set; }

        [DefaultValue(Theme.Light)]
        Theme ColorTheme { get; set; }

        ILayout HorizontalRuler { get; }

        ILayout VerticalRuler { get; }

        IColorPalette CustomColors { get; }
    }

    public interface ILayout
    {
        double Left { get; set; }
        double Top { get; set; }

        [DefaultValue(-1.0)]
        double Width { get; set; }

        [DefaultValue(-1.0)]
        double Height { get; set; }

        [DefaultValue(ZeroPoint.Near)]
        ZeroPoint ZeroPoint { get; set; }

        [DefaultValue(false)]
        bool Flip { get; set; }
    }

    public interface IColorPalette
    {
        SolidColorBrush Foreground { get; set; }
        SolidColorBrush Background { get; set; }
        SolidColorBrush Marker { get; set; }
        SolidColorBrush Angle { get; set; }
        SolidColorBrush Info { get; set; }
    }

}
