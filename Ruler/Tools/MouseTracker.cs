using System.Windows;
using System.Windows.Controls;

namespace Ruler
{
    public static class MouseTracker
    {
        public static Point GetTrackPoint(Window window, Orientation orientation, Edge edge)
        {
            var screenPoint = GetMousePosition(window);
            var clientPoint = new Point(screenPoint.X - window.Left, screenPoint.Y - window.Top);

            Rect rect = new(new Point(window.Left, window.Top), new Size(window.ActualWidth, window.ActualHeight));

            double x = 0, y = 0;
            if (orientation == Orientation.Horizontal)
            {
                x = ToScaleX(screenPoint, clientPoint, rect);

                // y is the distance from the scale edge to the mouse position
                if (edge == Edge.Top)
                {
                    y = window.Top - screenPoint.Y;
                }
                else
                {
                    y = window.Top + window.Height - screenPoint.Y;
                }

            }
            else if (orientation == Orientation.Vertical)
            {
                y = ToScaleY(screenPoint, clientPoint, rect);

                // x is the distance from the scale edge to the mouse position
                if (edge == Edge.Right)
                {
                    x = screenPoint.X - window.Left - window.Width;
                }
                else
                {
                    x = screenPoint.X - window.Left;
                }
            }

            return new(x, y);
        }

        public static Point GetMousePosition(Window window)
        {
            var pt = System.Windows.Forms.Control.MousePosition;
            var src = PresentationSource.FromVisual(window);
            if (src != null)
            {
                var transform = src.CompositionTarget.TransformFromDevice;
                return transform.Transform(new Point(pt.X, pt.Y));
            }
            return new();
        }

        private static double ToScaleX(Point screenPoint, Point clientPoint, Rect rect)
        {
            double x = 0;
            if (screenPoint.X > rect.Right)
            {
                x = rect.Width;
            }
            else if (screenPoint.X >= rect.Left)
            {
                x = clientPoint.X;
            }
            return x;
        }

        private static double ToScaleY(Point screenPoint, Point clientPoint, Rect rect)
        {
            double y = 0;
            if (screenPoint.Y > rect.Bottom)
            {
                y = rect.Height;
            }
            else if (screenPoint.Y >= rect.Top)
            {
                y = clientPoint.Y;
            }
            return y;
        }
    }
}
