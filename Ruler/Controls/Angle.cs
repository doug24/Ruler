using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruler
{
    public class Angle : Measure
    {
        static Angle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Angle),
                new FrameworkPropertyMetadata(typeof(Angle)));
        }

        public Point Origin
        {
            get { return (Point)GetValue(OriginProperty); }
            set { SetValue(OriginProperty, value); }
        }

        public static readonly DependencyProperty OriginProperty =
            DependencyProperty.Register("Origin", typeof(Point), typeof(Angle),
            new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));

        public Point MousePoint
        {
            get { return (Point)GetValue(MousePointProperty); }
            set { SetValue(MousePointProperty, value); }
        }

        public static readonly DependencyProperty MousePointProperty =
            DependencyProperty.Register("MousePoint", typeof(Point), typeof(Angle),
            new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));

        public double RulerWidth
        {
            get { return (double)GetValue(RulerWidthProperty); }
            set { SetValue(RulerWidthProperty, value); }
        }

        public static readonly DependencyProperty RulerWidthProperty =
            DependencyProperty.Register("RulerWidth", typeof(double), typeof(Measure),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public double RulerHeight
        {
            get { return (double)GetValue(RulerHeightProperty); }
            set { SetValue(RulerHeightProperty, value); }
        }

        public static readonly DependencyProperty RulerHeightProperty =
            DependencyProperty.Register("RulerHeight", typeof(double), typeof(Measure),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool Perpendicular
        {
            get { return (bool)GetValue(PerpendicularProperty); }
            set { SetValue(PerpendicularProperty, value); }
        }

        public static readonly DependencyProperty PerpendicularProperty =
            DependencyProperty.Register("Perpendicular", typeof(bool), typeof(Angle),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));


        protected override void OnRender(DrawingContext dc)
        {
            Window window = Window.GetWindow(this);
            Screen? screen = null;
            if (window != null)
            {
                screen = Screen.FromWindow(window);
                onePixelInDip = 1.0 / screen.ScaleX;
            }

            Pen linePen = new(RulerSettings.CurrentTheme.Angle, 1);
            linePen.Freeze();

            Pen foregroundPen = new(RulerSettings.CurrentTheme.Foreground, 1);
            foregroundPen.Freeze();

            Pen mousePen = new(RulerSettings.CurrentTheme.Mouse, onePixelInDip);
            mousePen.Freeze();

            SolidColorBrush transparentBrush = new()
            {
                Color = Colors.White,
                Opacity = 0.01
            };
            transparentBrush.Freeze();


            double x = Math.Min(Origin.X, MousePoint.X);
            double w = Math.Max(Origin.X, MousePoint.X) - x;
            double y = Math.Min(Origin.Y, MousePoint.Y);
            double h = Math.Max(Origin.Y, MousePoint.Y) - y;

            double dy = Origin.Y - MousePoint.Y;
            double dx = Origin.X - MousePoint.X;
            double radius = Math.Max(w, h) * 2 / 3;

            GetArcLayout(dy, dx, radius, out double startDegrees, out double sweepDegrees,
                out SweepDirection sweepDirection, out Point textOrigin, out FormattedText label);

            var originClient = OffsetPointToClient(Origin, screen);
            var mouseClient = OffsetPointToClient(MousePoint, screen);
            var trackClient = OffsetPointToClient(TrackPoint, screen);
            var textOriginClient = OffsetPointToClient(textOrigin, screen);

            dc.DrawCircularArc(null, linePen, originClient, radius,
                startDegrees, sweepDegrees, sweepDirection);

            Rect rulerRect = new(originClient, new Size(RulerWidth, RulerHeight));
            rulerRect.Inflate(-1, -1);
            if (!rulerRect.Contains(mouseClient))
            {
                // this keeps the mouse point 'within' the control and
                // not click through to other windows
                dc.DrawRectangle(transparentBrush, null, new(mouseClient.X - 16, mouseClient.Y - 16, 32, 32));
            }

            if (Perpendicular)
            {
                Point pt = Orientation == Orientation.Horizontal ?
                    new(mouseClient.X, originClient.Y) : new(originClient.X, mouseClient.Y);
                dc.DrawLine(mousePen, pt, mouseClient);

                // perpendicular offset text
                double offset = Orientation == Orientation.Horizontal ? trackClient.Y : trackClient.X;
                double offsetInUnits = DipConverter.Convert(offset, ScaleUnits, Orientation);
                var offsetMarker = FormatText(offsetInUnits, MarkerFontSize, RulerSettings.CurrentTheme.Mouse);
                Point markerPoint = GetPerpendicularTextLocation(offsetMarker, mouseClient, originClient);
                Rect offsetRect = new(markerPoint, new Size(offsetMarker.Width, offsetMarker.Height));
                offsetRect.Inflate(2, 2);
                dc.DrawRoundedRectangle(RulerSettings.CurrentTheme.Info, null, offsetRect, 2, 2);
                dc.DrawText(offsetMarker, markerPoint);
            }

            // hypotenuse
            dc.DrawLine(linePen, originClient, mouseClient);

            double hyp = Math.Sqrt(dx * dx + dy * dy);
            double hypInUnits = DipConverter.Convert(hyp, ScaleUnits, Orientation);
            var hypText = FormatText(hypInUnits, MarkerFontSize, RulerSettings.CurrentTheme.Foreground);
            Point hypPoint = GetHypotenuseTextLocation(hypText, mouseClient, trackClient);
            Rect hypTextRect = new(hypPoint, new Size(hypText.Width, hypText.Height));
            hypTextRect.Inflate(4, 4);
            dc.DrawRoundedRectangle(RulerSettings.CurrentTheme.Info, foregroundPen, hypTextRect, 4, 4);
            dc.DrawText(hypText, hypPoint);

            // angle text
            Rect textRect = new(textOriginClient, new Size(label.Width, label.Height));
            textRect.Inflate(4, 4);
            dc.DrawRoundedRectangle(RulerSettings.CurrentTheme.Info, foregroundPen, textRect, 4, 4);
            dc.DrawText(label, textOriginClient);
        }

        private static Point OffsetPointToClient(Point pt, Screen? screen)
        {
            if (screen == null) return pt;

            return new(pt.X - screen.WorkingAreaDip.Left, pt.Y - screen.WorkingAreaDip.Top);
        }

        private void GetArcLayout(double dy, double dx, double radius,
            out double startDegrees, out double sweepDegrees, out SweepDirection sweepDirection,
            out Point textOrigin, out FormattedText label)
        {
            Quadrant quad = (dy >= 0 && dx < 0) ? Quadrant.TopRight :
                (dy >= 0 && dx >= 0) ? Quadrant.TopLeft :
                (dy < 0 && dx >= 0) ? Quadrant.BottomLeft :
                Quadrant.BottomRight;

            bool textOffset = false;

            if (Orientation == Orientation.Horizontal)
            {
                double radians = dx != 0 ? Math.Atan(Math.Abs(dy) / Math.Abs(dx)) : 0;
                double angle = radians * 180 / Math.PI;

                // Horizontal with zero on the left
                if (ZeroPoint == ZeroPoint.Near)
                {
                    startDegrees = 0;

                    if (quad == Quadrant.TopRight)
                    {
                        sweepDegrees = angle;
                        sweepDirection = SweepDirection.Counterclockwise;
                    }
                    else if (quad == Quadrant.TopLeft)
                    {
                        sweepDegrees = 180 - angle;
                        sweepDirection = SweepDirection.Counterclockwise;
                    }
                    else if (quad == Quadrant.BottomLeft)
                    {
                        sweepDegrees = 180 - angle;
                        sweepDirection = SweepDirection.Clockwise;
                    }
                    else
                    {
                        sweepDegrees = angle;
                        sweepDirection = SweepDirection.Clockwise;
                    }
                }
                else // Horizontal with zero on the right
                {
                    startDegrees = 180;
                    textOffset = true;

                    if (quad == Quadrant.TopRight)
                    {
                        sweepDegrees = 180 - angle;
                        sweepDirection = SweepDirection.Clockwise;
                    }
                    else if (quad == Quadrant.TopLeft)
                    {
                        sweepDegrees = angle;
                        sweepDirection = SweepDirection.Clockwise;
                    }
                    else if (quad == Quadrant.BottomLeft)
                    {
                        sweepDegrees = angle;
                        sweepDirection = SweepDirection.Counterclockwise;
                    }
                    else
                    {
                        sweepDegrees = 180 - angle;
                        sweepDirection = SweepDirection.Counterclockwise;
                    }
                }
            }
            else // Vertical
            {
                double radians = dy != 0 ? Math.Atan(Math.Abs(dx) / Math.Abs(dy)) : Math.PI / 2;
                double angle = radians * 180 / Math.PI;

                // Vertical with zero at the top
                if (ZeroPoint == ZeroPoint.Near)
                {
                    startDegrees = 90; // vertical down

                    if (quad == Quadrant.TopRight)
                    {
                        sweepDegrees = 180 - angle;
                        sweepDirection = SweepDirection.Counterclockwise;
                    }
                    else if (quad == Quadrant.TopLeft)
                    {
                        sweepDegrees = 180 - angle;
                        sweepDirection = SweepDirection.Clockwise;
                        textOffset = true;
                    }
                    else if (quad == Quadrant.BottomLeft)
                    {
                        sweepDegrees = angle;
                        sweepDirection = SweepDirection.Clockwise;
                        textOffset = true;
                    }
                    else // BottomRight
                    {
                        sweepDegrees = angle;
                        sweepDirection = SweepDirection.Counterclockwise;
                    }
                }
                else // Vertical, with zero at the bottom
                {
                    startDegrees = -90; // vertical up

                    if (quad == Quadrant.TopRight)
                    {
                        sweepDegrees = angle;
                        sweepDirection = SweepDirection.Clockwise;
                    }
                    else if (quad == Quadrant.TopLeft)
                    {
                        sweepDegrees = angle;
                        sweepDirection = SweepDirection.Counterclockwise;
                        textOffset = true;
                    }
                    else if (quad == Quadrant.BottomLeft)
                    {
                        sweepDegrees = 180 - angle;
                        sweepDirection = SweepDirection.Counterclockwise;
                        textOffset = true;
                    }
                    else // BottomRight
                    {
                        sweepDegrees = 180 - angle;
                        sweepDirection = SweepDirection.Clockwise;
                    }
                }
            }

            label = FormatText(sweepDegrees.ToString("f2") + char.ConvertFromUtf32(0x00B0),
                MarkerFontSize, RulerSettings.CurrentTheme.Foreground);
            textOrigin = DrawingExtensions.GetEndPointOnRadus(Origin, radius + 12,
                startDegrees, sweepDegrees / 2, sweepDirection);
            textOrigin.Offset(textOffset ? -label.Width : 0, -label.Height / 2);
        }

        private Point GetPerpendicularTextLocation(FormattedText text, Point mouseClient, Point originClient)
        {
            Point pt;
            if (Orientation == Orientation.Horizontal)
            {
                pt = new(mouseClient.X + 6, (mouseClient.Y + originClient.Y) / 2);
            }
            else
            {
                pt = new((mouseClient.X + originClient.X) / 2, mouseClient.Y + 6);
            }

            return ConstraintToClient(text, pt);
        }

        private Point GetHypotenuseTextLocation(FormattedText text, Point mouseClient, Point trackClient)
        {
            Point pt;
            if (Orientation == Orientation.Horizontal)
            {
                pt = mouseClient;
                if (ZeroPoint == ZeroPoint.Near)
                {
                    pt.Offset(16, 0);
                }
                else
                {
                    pt.Offset(-text.Width - 16, 0);
                }
            }
            else
            {
                pt = mouseClient;
                if (trackClient.X > text.Width)
                {
                    pt.Offset(16, 12);
                }
                else
                {
                    pt.Offset(-text.Width - 8, 12);
                }
            }

            return ConstraintToClient(text, pt);
        }

        private Point ConstraintToClient(FormattedText text, Point pt)
        {
            if (pt.X < 4)
            {
                pt.X = 4;
            }
            else if (pt.X + text.Width > ActualWidth - 4)
            {
                pt.X = ActualWidth - text.Width - 4;
            }

            if (pt.Y < 4)
            {
                pt.Y = 4;
            }
            if (pt.Y + text.Height > ActualHeight - 4)
            {
                pt.Y = ActualHeight - text.Height - 4;
            }

            return pt;
        }
    }
}
