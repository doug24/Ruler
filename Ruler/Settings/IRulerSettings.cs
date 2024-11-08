﻿using System.ComponentModel;
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

        [DefaultValue(200d)]
        double MagnifierWidth { get; set; }

        [DefaultValue(200d)]
        double MagnifierHeight { get; set; }

        [DefaultValue(true)]
        bool Perpendicular { get; set; }

        [DefaultValue(Theme.Light)]
        Theme ColorTheme { get; set; }

        [DefaultValue("Segoe UI")]
        string FontFamily { get; set; }

        [DefaultValue(12d)]
        double FontSize { get; set; }

        [DefaultValue(12d)]
        double MarkerFontSize { get; set; }

        [DefaultValue(13d)]
        double DialogFontSize { get; set; }

        [DefaultValue(1d)]
        double Opacity { get; set; }

        [DefaultValue(true)]
        bool ShowBorders { get; set; }

        [DefaultValue(false)]
        bool ThinScale { get; set; }

        [DefaultValue(false)]
        bool TopMost { get; set; }

        [DefaultValue("")]
        string Markers { get; set; }

        ILayout HorizontalRuler { get; }

        ILayout VerticalRuler { get; }

        ILayoutSnapshot WindowSnapshot { get; set; }

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

        [DefaultValue(false)]
        bool ThinScale { get; set; }
    }

    public interface ILayoutSnapshot : ILayout
    {
        [DefaultValue(true)]
        bool IsEmpty { get; set; }

        Orientation Orientation { get; set; }
    }

    public interface IColorPalette
    {
        SolidColorBrush Foreground { get; set; }
        SolidColorBrush Background { get; set; }
        SolidColorBrush Mouse { get; set; }
        SolidColorBrush Marker { get; set; }
        SolidColorBrush Angle { get; set; }
        SolidColorBrush Info { get; set; }
    }

}
