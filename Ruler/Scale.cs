using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruler
{
    public class Scale : Measure
    {
        static Scale()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Scale),
                new FrameworkPropertyMetadata(typeof(Scale)));
        }

        public Scale()
        {
            SetValue(SnapsToDevicePixelsProperty, true);
            SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Application.Current.MainWindow != null)
            {
                Screen scr = Screen.FromWindow(Application.Current.MainWindow);
                onePixelInDip = 1.0 / scr.ScaleX;
            }

            Pen foregroundPen = new(RulerSettings.Default.Foreground, onePixelInDip);
            foregroundPen.Freeze();

            RenderBackground(drawingContext, foregroundPen);

            if (Orientation == Orientation.Horizontal)
            {
                RenderHorizontalScale(drawingContext, foregroundPen);
            }
            else
            {
                RenderVerticalScale(drawingContext, foregroundPen);
            }
        }

        private void RenderBackground(DrawingContext dc, Pen foregroundPen)
        {
            var width = ActualWidth;
            var height = ActualHeight;

            dc.DrawRectangle(RulerSettings.Default.Background, foregroundPen,
                new(new Point(0, 0), new Size(ActualWidth, ActualHeight)));
            dc.DrawLine(foregroundPen, new Point(0, 0.5), new Point(width, 0.5));
            dc.DrawLine(foregroundPen, new Point(0.5, 0), new Point(0.5, height));
        }

        private void RenderHorizontalScale(DrawingContext dc, Pen foregroundPen)
        {
            var width = ActualWidth;
            var height = ActualHeight;

            var tm = tickMap[ScaleUnits];
            double unitsSize = DipConverter.Convert(width, ScaleUnits, Orientation.Horizontal);

            for (double unit = 0; unit < unitsSize; unit += tm.Step)
            {
                double len = Modulo(unit, tm.LargeTick, tm.Step) ? 32 :
                    Modulo(unit, tm.MediumLargeTick, tm.Step) ? 24 :
                    Modulo(unit, tm.MediumTick, tm.Step) ? 16 :
                    Modulo(unit, tm.SmallTick, tm.Step) ? 8 : 0;

                if (len > 0)
                {
                    double dip = DipConverter.ToDIP(unit, width,
                        ZeroPoint, ScaleUnits, Orientation.Horizontal);
                    Drawline(dc, foregroundPen, dip, len, Orientation.Horizontal);
                }

                if (Modulo(unit, tm.LabelTick, tm.Step))
                {
                    double dip = DipConverter.ToDIP(unit, width,
                        ZeroPoint, ScaleUnits, Orientation.Horizontal);
                    var label = FormatText(unit);
                    double x = Math.Max(1, Math.Min(dip - label.Width / 2, width - label.Width - 1));
                    double y = Flip ? height - label.Height - 32 : 32;
                    dc.DrawText(label, new(x, y));
                }
            }

            // Mouse track point marker
            // 0 <= TrackPoint.X <= width, in Dips
            Drawline(dc, new Pen(RulerSettings.Default.MarkerBrush, onePixelInDip), TrackPoint.X, height, Orientation.Horizontal);

            double tx = DipConverter.ToDIP(TrackPoint.X, width,
                ZeroPoint, Units.DIP, Orientation.Horizontal);
            double txInUnits = DipConverter.Convert(tx, ScaleUnits, Orientation.Horizontal);
            double tyInUnits = DipConverter.Convert(TrackPoint.Y, ScaleUnits, Orientation.Vertical);

            var xmarker = FormatText(txInUnits, RulerSettings.Default.MarkerBrush);
            var ymarker = FormatText(tyInUnits, RulerSettings.Default.MarkerBrush);

            var xpos1 = TrackPoint.X - xmarker.Width - 6 < 0 ? TrackPoint.X + 1 : TrackPoint.X - xmarker.Width - 6;
            var ypos1 = Flip ? 0 : height - 2 * xmarker.Height;

            var xpos2 = TrackPoint.X - ymarker.Width - 6 < 0 ? TrackPoint.X + 1 : TrackPoint.X - ymarker.Width - 6;
            var ypos2 = Flip ? xmarker.Height : height - ymarker.Height;

            dc.DrawText(xmarker, new(xpos1, ypos1));
            dc.DrawText(ymarker, new(xpos2, ypos2));

            //Window w = Window.GetWindow(this);
            //var t1 = w != null ? FormatText(w.Left) : FormatText(0);
            //dc.DrawText(t1, new Point(1, height - t1.Height));
            //var t2 = w != null ? FormatText(w.Left + w.ActualWidth) : FormatText(width);
            //dc.DrawText(t2, new Point(width - t2.Width - 1, height - t1.Height));
        }

        private void RenderVerticalScale(DrawingContext dc, Pen foregroundPen)
        {
            var width = ActualWidth;
            var height = ActualHeight;

            var tm = tickMap[ScaleUnits];
            double unitsSize = DipConverter.Convert(height, ScaleUnits, Orientation.Vertical);

            for (double unit = 0; unit < unitsSize; unit += tm.Step)
            {
                double len = Modulo(unit, tm.LargeTick, tm.Step) ? 16 :
                    Modulo(unit, tm.MediumLargeTick, tm.Step) ? 12 :
                    Modulo(unit, tm.MediumTick, tm.Step) ? 8 :
                    Modulo(unit, tm.SmallTick, tm.Step) ? 4 : 0;

                if (len > 0)
                {
                    double dip = DipConverter.ToDIP(unit, height,
                        ZeroPoint, ScaleUnits, Orientation.Vertical);
                    Drawline(dc, foregroundPen, dip, len, Orientation.Vertical);
                }

                if (Modulo(unit, tm.LabelTick, tm.Step))
                {
                    double dip = DipConverter.ToDIP(unit, height,
                        ZeroPoint, ScaleUnits, Orientation.Vertical);
                    var label = FormatText(unit);
                    double x = Flip ? 20 : width - label.Width - 20;
                    double y = Math.Max(0, Math.Min(dip - label.Height / 2, height - label.Height));
                    dc.DrawText(label, new(x, y));
                }
            }

            // Mouse track point marker
            // 0 <= TrackPoint.Y <= height, in DIPs
            Drawline(dc, new Pen(RulerSettings.Default.MarkerBrush, onePixelInDip), TrackPoint.Y, width, Orientation.Vertical);

            double txInUnits = DipConverter.Convert(TrackPoint.X, ScaleUnits, Orientation.Vertical);
            double ty = DipConverter.ToDIP(TrackPoint.Y, height,
                ZeroPoint, Units.DIP, Orientation.Vertical);
            double tyInUnits = DipConverter.Convert(ty, ScaleUnits, Orientation.Vertical);

            var xmarker = FormatText(txInUnits, RulerSettings.Default.MarkerBrush);
            var ymarker = FormatText(tyInUnits, RulerSettings.Default.MarkerBrush);
            var measure = FormatText(333.3);

            var xpos = Flip ? width - measure.Width - 2 : 4;
            var ypos1 = TrackPoint.Y - 2 * ymarker.Height - 1 < 0 ?
                TrackPoint.Y + 1 : TrackPoint.Y - 2 * ymarker.Height - 1;
            var ypos2 = TrackPoint.Y - 2 * ymarker.Height - 1 < 0 ?
                TrackPoint.Y + ymarker.Height + 1 : TrackPoint.Y - ymarker.Height - 1;

            dc.DrawText(ymarker, new(xpos, ypos1));
            dc.DrawText(xmarker, new(xpos, ypos2));
        }
    }
}
