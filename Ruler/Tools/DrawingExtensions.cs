using System;
using System.Windows;
using System.Windows.Media;

namespace Ruler
{
    public static class DrawingExtensions
    {
        /// <summary>
        /// Calculates the coordinates of a point on a circle, at a given angle.
        /// </summary>
        /// <param name="center">The center-coordinates of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="angle">The angle in degrees at which to retrieve the coordinates</param>
        public static Point GetPointAtAngle(Point center, double radius, double angle)
        {
            double x = center.X + (radius * Math.Cos(angle / 180 * Math.PI));
            double y = center.Y + (radius * Math.Sin(angle / 180 * Math.PI));

            return new Point(x, y);
        }

        /// <summary>
        /// Draws a semi-circular arc with a sweep angle of 0..180 
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="brush"></param>
        /// <param name="pen"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="startDegrees"></param>
        /// <param name="sweepDegrees"></param>
        /// <param name="sweepDirection"></param>
        public static void DrawCircularArc(this DrawingContext dc, Brush? brush, Pen pen,
          Point center, double radius, double startDegrees, double sweepDegrees,
          SweepDirection sweepDirection)
        {
            while (startDegrees > 360)
            {
                startDegrees -= 360;
            }

            sweepDegrees = Math.Abs(sweepDegrees);
            while (sweepDegrees > 180)
            {
                sweepDegrees -= 180;
            }

            if (sweepDirection == SweepDirection.Counterclockwise)
            {
                sweepDegrees *= -1;
            }

            double endDegrees = startDegrees + sweepDegrees;

            Point start = GetPointAtAngle(center, radius, startDegrees);
            Point end = GetPointAtAngle(center, radius, endDegrees);

            var figure = new PathFigure
            {
                StartPoint = start
            };

            figure.Segments.Add(new ArcSegment
            {
                Point = end,
                Size = new Size(radius, radius),   // radii
                SweepDirection = sweepDirection
            });

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            dc.DrawGeometry(brush, pen, geometry);
        }

        /// <summary>
        /// Gets a point on radius using a start angle and a sweep
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="startDegrees"></param>
        /// <param name="sweepDegrees"></param>
        /// <param name="sweepDirection"></param>
        /// <returns></returns>
        public static Point GetEndPointOnRadus(Point center, double radius, double startDegrees, double sweepDegrees,
            SweepDirection sweepDirection)
        {
            while (startDegrees > 360)
            {
                startDegrees -= 360;
            }

            sweepDegrees = Math.Abs(sweepDegrees);
            while (sweepDegrees > 180)
            {
                sweepDegrees -= 180;
            }

            if (sweepDirection == SweepDirection.Counterclockwise)
            {
                sweepDegrees *= -1;
            }

            double endDegrees = startDegrees + sweepDegrees;

            Point end = GetPointAtAngle(center, radius, endDegrees);
            return end;
        }

    }
}
