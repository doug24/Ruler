using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ruler
{
    public class Scale : Measure
    {
        private Rect hotSpot = new(0, 0, 20, 80);
        private Orientation? previous = null;
        private readonly ToolTip toolTip = new();
        private readonly DispatcherTimer timer = new DispatcherTimer();

        static Scale()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Scale),
                new FrameworkPropertyMetadata(typeof(Scale)));
        }

        public Scale()
        {
            SetValue(SnapsToDevicePixelsProperty, true);
            SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;

            Loaded += Scale_Loaded;
            PreviewMouseMove += Scale_PreviewMouseMove;
        }

        private void Scale_Loaded(object sender, RoutedEventArgs e)
        {
            Window parent = Window.GetWindow(this);
            if (parent != null)
            {
                Binding binding = new("ScaleToolTip");
                binding.Source = parent.DataContext;
                toolTip.SetBinding(ContentControl.ContentProperty, binding);

                parent.Closing += (s, e) =>
                {
                    toolTip.IsOpen = false;
                    timer.Stop();
                };
            }
        }

        private void Scale_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (toolTip != null)
            {
                // Retrieve the coordinate of the mouse position.
                Point pt = e.GetPosition(this);
                if (hotSpot.Contains(pt))
                {
                    toolTip.IsOpen = true;
                    timer.Start();
                }
                else
                {
                    toolTip.IsOpen = false;
                    timer.Stop();
                }
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();
            toolTip.IsOpen = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Application.Current.MainWindow != null)
            {
                Screen scr = Screen.FromWindow(Application.Current.MainWindow);
                onePixelInDip = 1.0 / scr.ScaleX;
            }

            Pen foregroundPen = new(RulerSettings.CurrentTheme.Foreground, onePixelInDip);
            foregroundPen.Freeze();

            if (Orientation != previous)
            {
                previous = Orientation;

                toolTip.IsOpen = false;
                double width = Orientation == Orientation.Horizontal ? 20 : ActualWidth;
                double height = Orientation == Orientation.Vertical ? 20 : ActualHeight;
                hotSpot = new(0, 0, width, height);
            }

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

            dc.DrawRectangle(RulerSettings.CurrentTheme.Background, foregroundPen,
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
            Drawline(dc, new Pen(RulerSettings.CurrentTheme.Marker, onePixelInDip), TrackPoint.X, height, Orientation.Horizontal);

            double tx = DipConverter.ToDIP(TrackPoint.X, width,
                ZeroPoint, Units.DIP, Orientation.Horizontal);
            double txInUnits = DipConverter.Convert(tx, ScaleUnits, Orientation.Horizontal);
            var xmarker = FormatText(txInUnits, RulerSettings.CurrentTheme.Marker);

            var xpos1 = TrackPoint.X - xmarker.Width - 6 < 0 ? TrackPoint.X + 1 : TrackPoint.X - xmarker.Width - 6;
            var ypos1 = Flip ? xmarker.Height + 2 : height - 2 * xmarker.Height;

            dc.DrawText(xmarker, new(xpos1, ypos1));

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
            Drawline(dc, new Pen(RulerSettings.CurrentTheme.Marker, onePixelInDip), TrackPoint.Y, width, Orientation.Vertical);

            double ty = DipConverter.ToDIP(TrackPoint.Y, height,
                ZeroPoint, Units.DIP, Orientation.Vertical);
            double tyInUnits = DipConverter.Convert(ty, ScaleUnits, Orientation.Vertical);
            var ymarker = FormatText(tyInUnits, RulerSettings.CurrentTheme.Marker);

            var xpos = Flip ? width - ymarker.Width - 4 : 4;
            var ypos1 = TrackPoint.Y - ymarker.Height - 1 < 0 ?
                TrackPoint.Y + 1 : TrackPoint.Y - ymarker.Height - 1;

            dc.DrawText(ymarker, new(xpos, ypos1));
        }
    }
}
