using System;
using System.Windows;
using System.Windows.Media;

namespace Ruler
{
    public static class DrawingExtensions
    {
        /// <summary>
        /// Draw an Arc of an ellipse or circle. Static extension method of DrawingContext.
        /// </summary>
        /// <param name="dc">DrawingContext</param>
        /// <param name="pen">Pen for outline. set to null for no outline.</param>
        /// <param name="brush">Brush for fill. set to null for no fill.</param>
        /// <param name="rect">Box to hold the whole ellipse described by the arc</param>
        /// <param name="startDegrees">Start angle of the arc degrees within the ellipse. 0 degrees is a line to the right.</param>
        /// <param name="sweepDegrees">Sweep angle, -ve = Counterclockwise, +ve = Clockwise</param>
        public static void DrawArc(this DrawingContext dc, Pen pen, Brush brush, Rect rect, double startDegrees, double sweepDegrees)
        {
            GeometryDrawing arc = CreateArcDrawing(rect, startDegrees, sweepDegrees);
            dc.DrawGeometry(brush, pen, arc.Geometry);
        }

        /// <summary>
        /// Create an Arc geometry drawing of an ellipse or circle
        /// </summary>
        /// <param name="rect">Box to hold the whole ellipse described by the arc</param>
        /// <param name="startDegrees">Start angle of the arc degrees within the ellipse. 0 degrees is a line to the right.</param>
        /// <param name="sweepDegrees">Sweep angle, -ve = Counterclockwise, +ve = Clockwise</param>
        /// <returns>GeometryDrawing object</returns>
        private static GeometryDrawing CreateArcDrawing(Rect rect, double startDegrees, double sweepDegrees)
        {
            // degrees to radians conversion
            double startRadians = startDegrees * Math.PI / 180.0;
            double sweepRadians = sweepDegrees * Math.PI / 180.0;

            // x and y radius
            double dx = rect.Width / 2;
            double dy = rect.Height / 2;

            // determine the start point 
            double xs = rect.X + dx + (Math.Cos(startRadians) * dx);
            double ys = rect.Y + dy + (Math.Sin(startRadians) * dy);

            // determine the end point 
            double xe = rect.X + dx + (Math.Cos(startRadians + sweepRadians) * dx);
            double ye = rect.Y + dy + (Math.Sin(startRadians + sweepRadians) * dy);

            // draw the arc into a stream geometry
            StreamGeometry streamGeom = new();
            using (StreamGeometryContext ctx = streamGeom.Open())
            {
                bool isLargeArc = Math.Abs(sweepDegrees) > 180;
                SweepDirection sweepDirection = sweepDegrees < 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

                ctx.BeginFigure(new Point(xs, ys), false, false);
                ctx.ArcTo(new Point(xe, ye), new Size(dx, dy), 0, isLargeArc, sweepDirection, true, false);
            }

            // create the drawing
            GeometryDrawing drawing = new()
            {
                Geometry = streamGeom
            };
            return drawing;
        }

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
            //Rect rect = new(new Point(center.X - radius, center.Y - radius),
            //    new Size(2 * radius, 2 * radius));

            //dc.DrawRectangle(null, new Pen(Brushes.Green, 1), rect);

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
