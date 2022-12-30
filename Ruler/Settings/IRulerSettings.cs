using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruler
{
    public interface IRulerSettings
    {
        [DefaultValue(Orientation.Horizontal)]
        Orientation Orientation { get; set; }

        [DefaultValue(Units.DIP)]
        Units ScaleUnits { get; set; }

        [DefaultValue(1.6f)]
        float Magnification { get; set; }

        SolidColorBrush Foreground { get; set; }

        SolidColorBrush Background { get; set; }

        SolidColorBrush MarkerBrush { get; set; }
        SolidColorBrush AngleBrush { get; set;}
        SolidColorBrush InfoBrush { get; set; }

        ILayout HorizontalRuler { get; }
        ILayout VerticalRuler { get; }
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

}
