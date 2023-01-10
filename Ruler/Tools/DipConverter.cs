using System;
using System.Windows;
using System.Windows.Controls;

namespace Ruler
{
    public static class DipConverter
    {
        public static double Convert(double dip, Units units, Orientation orientation)
        {
            switch (units)
            {
                case Units.Pixel:
                    if (orientation == Orientation.Horizontal)
                    {
                        return DipsToPixelsX(dip);
                    }
                    else
                    {
                        return DipsToPixelsY(dip);
                    }

                default:
                case Units.DIP:
                    return dip;

                case Units.Point:
                    return Math.Ceiling(dip * 72.0 / 96.0);

                case Units.Inch:
                    return dip / 96.0;

                case Units.CM:
                    return Math.Round(dip * 2.54 / 96.0, 2);

                case Units.Percent:
                    var w = Application.Current.MainWindow;
                    Point pt = new(w.Left + w.ActualWidth / 2, w.Top + w.ActualHeight / 2);
                    Screen screen = Screen.FromPoint(pt);
                    if (orientation == Orientation.Horizontal)
                    {
                        return Math.Round(dip * 100.0 / screen.WorkingAreaDip.Width, 2);
                    }
                    else
                    {
                        return Math.Round(dip * 100.0 / screen.WorkingAreaDip.Height, 2);
                    }
            }
        }

        public static double ToDIP(double value, Units units, Orientation orientation)
        {
            switch (units)
            {
                case Units.Pixel:
                    if (orientation == Orientation.Horizontal)
                    {
                        return PixelsToDipsX(value);
                    }
                    else
                    {
                        return PixelsToDipsY(value);
                    }

                default:
                case Units.DIP:
                    return value;

                case Units.Point:
                    return value * 96.0 / 72.0;

                case Units.Inch:
                    return value * 96.0;

                case Units.CM:
                    return value * 96.0 / 2.54;

                case Units.Percent:
                    var w = Application.Current.MainWindow;
                    Point pt = new(w.Left + w.ActualWidth / 2, w.Top + w.ActualHeight / 2);
                    Screen screen = Screen.FromPoint(pt);
                    if (orientation == Orientation.Horizontal)
                    {
                        return value * screen.WorkingAreaDip.Width / 100.0;
                    }
                    else
                    {
                        return value * screen.WorkingAreaDip.Height / 100.0;
                    }
            }
        }

        public static double ToDIP(double dip, double maxValue, ZeroPoint zeroPoint, Units units, Orientation orientation)
        {
            // return a value in the range 0..width

            double valueInDip = ToDIP(dip, units, orientation);

            if (zeroPoint == ZeroPoint.Near)
            {
                return valueInDip;
            }

            return maxValue - valueInDip;
        }

        public static double DipsToPixelsX(double input)
        {
            if (Application.Current.MainWindow == null) return input;

            return input * PresentationSource.FromVisual(Application.Current.MainWindow)
                .CompositionTarget.TransformToDevice.M11;
        }

        public static double DipsToPixelsY(double input)
        {
            if (Application.Current.MainWindow == null) return input;

            return input * PresentationSource.FromVisual(Application.Current.MainWindow)
                .CompositionTarget.TransformToDevice.M22;
        }

        public static double PixelsToDipsX(double input)
        {
            if (Application.Current.MainWindow == null) return input;

            return input / PresentationSource.FromVisual(Application.Current.MainWindow)
                .CompositionTarget.TransformToDevice.M11;
        }

        public static double PixelsToDipsY(double input)
        {
            if (Application.Current.MainWindow == null) return input;

            return input / PresentationSource.FromVisual(Application.Current.MainWindow)
                .CompositionTarget.TransformToDevice.M22;
        }

    }
}
