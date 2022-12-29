using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruler
{
    public enum ZeroPoint
    {
        Near,
        Far
    }

    public enum Edge
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public enum Units
    {
        Pixel,
        DIP,
        Point,
        CM,
        Inch,
        Percent
    }

    public enum Quadrant
    {
        TopRight,
        TopLeft,
        BottomLeft,
        BottomRight,
    }


    public class Measure : Control
    {
        static Measure()
        {
        }

        protected double onePixelInDip = 1;

        protected readonly Dictionary<Units, ScaleTicks> tickMap = new()
        {
            {Units.Pixel, new(0.5, 5d, 10d, double.MaxValue, 100d, 100d)},
            {Units.DIP, new(1d, 5d, 10d, double.MaxValue, 50d, 50d)},
            {Units.Point, new(0.5, 5d, 10d, double.MaxValue, 50, 50d)},
            {Units.Inch, new(1d/16d, 1d/16d, 1d/4d, 1d/2d, 1d, 1d)},
            {Units.CM, new(1d/500d, double.MaxValue, 1d/10d, double.MaxValue, 1d, 1d)},
            {Units.Percent, new(0.5, 0.5d, 1d, double.MaxValue, 5d, 5d)},
        };

        protected readonly static Dictionary<int, string> Denominators = new()
        {
            {2, char.ConvertFromUtf32(0x2082)},
            {4, char.ConvertFromUtf32(0x2084)},
            {8, char.ConvertFromUtf32(0x2088)},
            {16, char.ConvertFromUtf32(0x2081) + char.ConvertFromUtf32(0x2086)},
            {32, char.ConvertFromUtf32(0x2083) + char.ConvertFromUtf32(0x2082)},
            {64, char.ConvertFromUtf32(0x2086) + char.ConvertFromUtf32(0x2084)},
        };

        protected readonly static string[] Superscripts = new[]
        {
            char.ConvertFromUtf32(0x2070),
            char.ConvertFromUtf32(0x00B9),
            char.ConvertFromUtf32(0x00B2),
            char.ConvertFromUtf32(0x00B3),
            char.ConvertFromUtf32(0x2074),
            char.ConvertFromUtf32(0x2075),
            char.ConvertFromUtf32(0x2076),
            char.ConvertFromUtf32(0x2077),
            char.ConvertFromUtf32(0x2078),
            char.ConvertFromUtf32(0x2079),
        };

        protected static string Numerator(int numerator)
        {
            var digits = GetIntArray(numerator);
            string result = string.Empty;
            foreach (int digit in digits)
            {
                result += Superscripts[digit];
            }
            return result;
        }

        protected static int[] GetIntArray(int num)
        {
            List<int> listOfInts = new();
            while (num > 0)
            {
                listOfInts.Add(num % 10);
                num /= 10;
            }
            listOfInts.Reverse();
            return listOfInts.ToArray();
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(Measure),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsRender));

        public Units ScaleUnits
        {
            get { return (Units)GetValue(ScaleUnitsProperty); }
            set { SetValue(ScaleUnitsProperty, value); }
        }

        public static readonly DependencyProperty ScaleUnitsProperty =
            DependencyProperty.Register("ScaleUnits", typeof(Units), typeof(Measure),
            new FrameworkPropertyMetadata(Units.DIP, FrameworkPropertyMetadataOptions.AffectsRender));

        public ZeroPoint ZeroPoint
        {
            get { return (ZeroPoint)GetValue(ZeroPointProperty); }
            set { SetValue(ZeroPointProperty, value); }
        }

        public static readonly DependencyProperty ZeroPointProperty =
            DependencyProperty.Register("ZeroPoint", typeof(ZeroPoint), typeof(Measure),
            new FrameworkPropertyMetadata(ZeroPoint.Near, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool Flip
        {
            get { return (bool)GetValue(FlipProperty); }
            set { SetValue(FlipProperty, value); }
        }

        public static readonly DependencyProperty FlipProperty =
            DependencyProperty.Register("Flip", typeof(bool), typeof(Measure),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public Point TrackPoint
        {
            get { return (Point)GetValue(TrackPointProperty); }
            set { SetValue(TrackPointProperty, value); }
        }

        public static readonly DependencyProperty TrackPointProperty =
            DependencyProperty.Register("TrackPoint", typeof(Point), typeof(Measure),
            new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));


        protected void Drawline(DrawingContext dc, Pen pen,
            double location, double length, Orientation orientation)
        {
            var width = ActualWidth;
            var height = ActualHeight;

            if (orientation == Orientation.Horizontal)
            {
                double y1 = Flip ? height : 0;
                double y2 = Flip ? height - length : length;
                dc.DrawLine(pen,
                    new Point(location + onePixelInDip, y1 + onePixelInDip),
                    new Point(location + onePixelInDip, y2 + onePixelInDip));
            }
            else
            {
                double x1 = Flip ? 0 : width - length;
                double x2 = Flip ? length : width;

                dc.DrawLine(pen,
                    new Point(x1 + onePixelInDip, location + onePixelInDip),
                    new Point(x2 + onePixelInDip, location + onePixelInDip));

            }
        }

        protected Typeface Typeface => new(FontFamily, FontStyle, FontWeight, FontStretch);

        protected FormattedText FormatText(double num)
        {
            if (ScaleUnits == Units.Inch)
            {
                return FormatFraction(num, 32, Foreground);
            }
            else if (ScaleUnits == Units.CM)
            {
                num = Math.Round(num, 2);
            }
            else if (ScaleUnits == Units.Percent)
            {
                num = Math.Round(num, 1);
            }
            return FormatText(num.ToString());
        }

        protected FormattedText FormatText(double num, Brush foreground)
        {
            if (ScaleUnits == Units.Inch)
            {
                return FormatFraction(num, 32, foreground);
            }
            else if (ScaleUnits == Units.CM)
            {
                num = Math.Round(num, 2);
            }
            else if (ScaleUnits == Units.Percent)
            {
                num = Math.Round(num, 1);
            }
            return FormatText(num.ToString(), foreground);
        }

        protected FormattedText FormatText(string text)
        {
            return FormatText(text, Foreground);
        }

        protected FormattedText FormatText(string text, Brush foreground)
        {
            return new(text,
                CultureInfo.CurrentCulture,
                FlowDirection,
                Typeface,
                FontSize,
                foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }

        protected FormattedText FormatFraction(double num, int maxDenominator, Brush foreground)
        {
            // Calculating the nearest increment of the value
            // argument based on the denominator argument.
            double incValue = Math.Round(num * maxDenominator) / maxDenominator;

            // Identifying the whole number of the argument value.
            int wholeValue = (int)Math.Truncate(incValue);

            // Calculating the remainder of the argument value and the whole value.
            double remainder = incValue - wholeValue;

            // Checking for the whole number case and returning if found.
            if (remainder == 0.0)
            {
                return FormatText(wholeValue.ToString(), foreground);
            }

            string sign = Math.Sign(remainder) == -1 ? "-" : string.Empty;

            // Iterating through the exponents of base 2 values until the
            // maximum denominator value has been reached or until the modulus
            // of the divisor.
            for (int i = 1; i < (int)Math.Log(maxDenominator, 2) + 1; i++)
            {
                // Calculating the denominator of the current iteration
                double denominator = Math.Pow(2, i);

                // Calculating the divisor increment value
                double divisor = Math.Pow(denominator, -1);

                // Checking if the current denominator evenly divides the remainder
                if ((remainder % divisor) == 0.0) // If, yes
                {
                    // Calculating the numerator of the remainder 
                    // given the calculated denominator
                    int numerator = Convert.ToInt32(remainder * denominator);

                    // Returning the resulting string from the conversion.
                    return FormatText(
                        (wholeValue != 0 ? wholeValue.ToString() + " " : sign) +
                        Numerator(Math.Abs(numerator)) +
                        char.ConvertFromUtf32(0x2044) +
                        Denominators[(int)denominator],
                        foreground);
                }
            }

            // Returns Error if something goes wrong.
            return FormatText("Error", foreground);
        }

        protected static bool Modulo(double a, double b, double maxdelta = 0.5)
        {
            if (double.IsNaN(a) || double.IsNaN(b) || b == 0)
            {
                throw new Exception("Modulo called with a or b == NaN or b == 0");
            }

            if (b == double.MaxValue)
            {
                return false;
            }

            double int_closest_to_ratio = Math.Round(a / b);
            double residue = a - int_closest_to_ratio * b;
            bool result = residue <= 0 && Math.Abs(residue) < maxdelta;
            return result;
        }
    }
}
