using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruler
{
    public enum Quadrant
    {
        TopRight,
        TopLeft,
        BottomLeft,
        BottomRight,
    }

    public class Angle : Control
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

        public RulerStyle RulerStyle
        {
            get { return (RulerStyle)GetValue(RulerStyleProperty); }
            set { SetValue(RulerStyleProperty, value); }
        }

        public static readonly DependencyProperty RulerStyleProperty =
            DependencyProperty.Register("RulerStyle", typeof(RulerStyle), typeof(Angle),
            new FrameworkPropertyMetadata(RulerStyle.Horizontal, FrameworkPropertyMetadataOptions.AffectsRender));

        public Units ScaleUnits
        {
            get { return (Units)GetValue(ScaleUnitsProperty); }
            set { SetValue(ScaleUnitsProperty, value); }
        }

        public static readonly DependencyProperty ScaleUnitsProperty =
            DependencyProperty.Register("ScaleUnits", typeof(Units), typeof(Angle),
            new FrameworkPropertyMetadata(Units.DIP, FrameworkPropertyMetadataOptions.AffectsRender));

        public ZeroPoint ZeroPoint
        {
            get { return (ZeroPoint)GetValue(ZeroPointProperty); }
            set { SetValue(ZeroPointProperty, value); }
        }

        public static readonly DependencyProperty ZeroPointProperty =
            DependencyProperty.Register("ZeroPoint", typeof(ZeroPoint), typeof(Angle),
            new FrameworkPropertyMetadata(ZeroPoint.Near, FrameworkPropertyMetadataOptions.AffectsRender));

        protected override void OnRender(DrawingContext dc)
        {
            Pen linePen = new(Foreground, 1);
            linePen.Freeze();

            Pen blackPen = new(Brushes.Black, 1);
            linePen.Freeze();

            double x = Math.Min(Origin.X, MousePoint.X);
            double w = Math.Max(Origin.X, MousePoint.X) - x;
            double y = Math.Min(Origin.Y, MousePoint.Y);
            double h = Math.Max(Origin.Y, MousePoint.Y) - y;

            double dy = Origin.Y - MousePoint.Y;
            double dx = Origin.X - MousePoint.X;
            double radius = Math.Max(w, h) * 2 / 3;

            GetArcLayout(dy, dx, radius, out double startDegrees, out double sweepDegrees,
                out SweepDirection sweepDirection, out Point textOrigin, out FormattedText label);

            Rect textRect = new(textOrigin, new Size(label.Width, label.Height));
            textRect.Inflate(4, 4);
            dc.DrawRoundedRectangle(SystemColors.InfoBrush, blackPen, textRect, 4, 4);
            dc.DrawText(label, textOrigin);

            dc.DrawCircularArc(null, linePen, Origin, radius,
                startDegrees, sweepDegrees, sweepDirection);

            dc.DrawLine(linePen, Origin, MousePoint);
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

            if (RulerStyle == RulerStyle.Horizontal)
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

            label = FormatText(sweepDegrees.ToString("f2") + char.ConvertFromUtf32(0x00B0), Brushes.Black);
            textOrigin = DrawingExtensions.GetEndPointOnRadus(Origin, radius + 12,
                startDegrees, sweepDegrees / 2, sweepDirection);
            textOrigin.Offset(textOffset ? -label.Width : 0, -label.Height / 2);
        }

        private Typeface Typeface => new(FontFamily, FontStyle, FontWeight, FontStretch);

        private FormattedText FormatText(string text, Brush foreground)
        {
            return new(text,
                CultureInfo.CurrentCulture,
                FlowDirection,
                Typeface,
                FontSize,
                foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }
    }
}
