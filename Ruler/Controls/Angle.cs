﻿using System;
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
            if (Application.Current.MainWindow != null)
            {
                Screen scr = Screen.FromWindow(Application.Current.MainWindow);
                onePixelInDip = 1.0 / scr.ScaleX;
            }

            Pen linePen = new(RulerSettings.CurrentTheme.Angle, 1);
            linePen.Freeze();

            Pen foregroundPen = new(RulerSettings.CurrentTheme.Foreground, 1);
            foregroundPen.Freeze();

            Pen markerPen = new(RulerSettings.CurrentTheme.Marker, onePixelInDip);
            markerPen.Freeze();

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

            dc.DrawCircularArc(null, linePen, Origin, radius,
                startDegrees, sweepDegrees, sweepDirection);

            // this keeps the mouse point 'within' the control and
            // not click through to other windows
            dc.DrawRectangle(transparentBrush, null, new(MousePoint.X - 16, MousePoint.Y - 16, 32, 32));

            // hypotenuse
            dc.DrawLine(linePen, Origin, MousePoint);

            if (Perpendicular)
            {
                Point pt = Orientation == Orientation.Horizontal ?
                    new(MousePoint.X, Origin.Y) : new(Origin.X, MousePoint.Y);
                dc.DrawLine(markerPen, pt, MousePoint);

                // perpendicular offset text
                double offset = Orientation == Orientation.Horizontal ? TrackPoint.Y : TrackPoint.X;
                double offsetInUnits = DipConverter.Convert(offset, ScaleUnits, Orientation);
                var offsetMarker = FormatText(offsetInUnits, RulerSettings.CurrentTheme.Marker);
                Point markerPoint = Orientation == Orientation.Horizontal ?
                    new(MousePoint.X + 6, (MousePoint.Y + Origin.Y) / 2) :
                    new((MousePoint.X + Origin.X) / 2, MousePoint.Y + 6);
                Rect offsetRect = new(markerPoint, new Size(offsetMarker.Width, offsetMarker.Height));
                offsetRect.Inflate(2, 2);
                dc.DrawRoundedRectangle(RulerSettings.CurrentTheme.Info, null, offsetRect, 2, 2);
                dc.DrawText(offsetMarker, markerPoint);
            }

            // angle text
            Rect textRect = new(textOrigin, new Size(label.Width, label.Height));
            textRect.Inflate(4, 4);
            dc.DrawRoundedRectangle(RulerSettings.CurrentTheme.Info, foregroundPen, textRect, 4, 4);
            dc.DrawText(label, textOrigin);
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

            label = FormatText(sweepDegrees.ToString("f2") + char.ConvertFromUtf32(0x00B0), RulerSettings.CurrentTheme.Foreground);
            textOrigin = DrawingExtensions.GetEndPointOnRadus(Origin, radius + 12,
                startDegrees, sweepDegrees / 2, sweepDirection);
            textOrigin.Offset(textOffset ? -label.Width : 0, -label.Height / 2);
        }
    }
}