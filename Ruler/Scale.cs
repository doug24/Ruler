using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ruler
{
    public enum RulerStyle
    {
        Horizontal,
        Vertical,
    }

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

    public class Scale : Control
    {
        static Scale()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Scale),
                new FrameworkPropertyMetadata(typeof(Scale)));
        }

        private double onePixelInDip = 1;

        private readonly Dictionary<Units, ScaleTicks> tickMap = new()
        {
            {Units.Pixel, new(0.5, 5d, 10d, double.MaxValue, 100d, 100d)},
            {Units.DIP, new(1d, 5d, 10d, double.MaxValue, 50d, 50d)},
            {Units.Point, new(0.5, 5d, 10d, double.MaxValue, 50, 50d)},
            {Units.Inch, new(1d/16d, 1d/16d, 1d/4d, 1d/2d, 1d, 1d)},
            {Units.CM, new(1d/500d, double.MaxValue, 1d/10d, double.MaxValue, 1d, 1d)},
            {Units.Percent, new(0.5, 0.5d, 1d, double.MaxValue, 5d, 5d)},
        };

        private readonly static Dictionary<int, string> Denominators = new()
        {
            {2, char.ConvertFromUtf32(0x2082)},
            {4, char.ConvertFromUtf32(0x2084)},
            {8, char.ConvertFromUtf32(0x2088)},
            {16, char.ConvertFromUtf32(0x2081) + char.ConvertFromUtf32(0x2086)},
            {32, char.ConvertFromUtf32(0x2083) + char.ConvertFromUtf32(0x2082)},
            {64, char.ConvertFromUtf32(0x2086) + char.ConvertFromUtf32(0x2084)},
        };

        private readonly static string[] Superscripts = new[]
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

        private static string Numerator(int numerator)
        {
            var digits = GetIntArray(numerator);
            string result = string.Empty;
            foreach (int digit in digits)
            {
                result += Superscripts[digit];
            }
            return result;
        }

        private static int[] GetIntArray(int num)
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

        public Scale()
        {
            SetValue(SnapsToDevicePixelsProperty, true);
            SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        }

        public RulerStyle RulerStyle
        {
            get { return (RulerStyle)GetValue(RulerStyleProperty); }
            set { SetValue(RulerStyleProperty, value); }
        }

        public static readonly DependencyProperty RulerStyleProperty =
            DependencyProperty.Register("RulerStyle", typeof(RulerStyle), typeof(Scale),
            new FrameworkPropertyMetadata(RulerStyle.Horizontal, FrameworkPropertyMetadataOptions.AffectsRender));

        public Units ScaleUnits
        {
            get { return (Units)GetValue(ScaleUnitsProperty); }
            set { SetValue(ScaleUnitsProperty, value); }
        }

        public static readonly DependencyProperty ScaleUnitsProperty =
            DependencyProperty.Register("ScaleUnits", typeof(Units), typeof(Scale),
            new FrameworkPropertyMetadata(Units.DIP, FrameworkPropertyMetadataOptions.AffectsRender));

        public ZeroPoint ZeroPoint
        {
            get { return (ZeroPoint)GetValue(ZeroPointProperty); }
            set { SetValue(ZeroPointProperty, value); }
        }

        public static readonly DependencyProperty ZeroPointProperty =
            DependencyProperty.Register("ZeroPoint", typeof(ZeroPoint), typeof(Scale),
            new FrameworkPropertyMetadata(ZeroPoint.Near, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool Flip
        {
            get { return (bool)GetValue(FlipProperty); }
            set { SetValue(FlipProperty, value); }
        }

        public static readonly DependencyProperty FlipProperty =
            DependencyProperty.Register("Flip", typeof(bool), typeof(Scale),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public Point TrackPoint
        {
            get { return (Point)GetValue(TrackPointProperty); }
            set { SetValue(TrackPointProperty, value); }
        }

        public static readonly DependencyProperty TrackPointProperty =
            DependencyProperty.Register("TrackPoint", typeof(Point), typeof(Scale),
            new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));


        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Application.Current.MainWindow != null)
            {
                Screen scr = Screen.FromWindow(Application.Current.MainWindow);
                onePixelInDip = 1.0 / scr.ScaleX;
            }

            Pen blackPen = new(Foreground, onePixelInDip);
            blackPen.Freeze();

            RenderBackground(drawingContext, blackPen);

            if (RulerStyle == RulerStyle.Horizontal)
            {
                RenderHorizontalScale(drawingContext, blackPen);
            }
            else if (RulerStyle == RulerStyle.Vertical)
            {
                RenderVerticalScale(drawingContext, blackPen);
            }
        }

        private void RenderBackground(DrawingContext dc, Pen blackPen)
        {
            var width = ActualWidth;
            var height = ActualHeight;

            dc.DrawRectangle(Brushes.WhiteSmoke, blackPen,
                new(new Point(0, 0), new Size(ActualWidth, ActualHeight)));
            dc.DrawLine(blackPen, new Point(0, 0.5), new Point(width, 0.5));
            dc.DrawLine(blackPen, new Point(0.5, 0), new Point(0.5, height));
        }

        private void RenderHorizontalScale(DrawingContext dc, Pen blackPen)
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
                    Drawline(dc, blackPen, dip, len, Orientation.Horizontal);
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
            Drawline(dc, new Pen(Brushes.ForestGreen, onePixelInDip), TrackPoint.X, height, Orientation.Horizontal);

            double tx = DipConverter.ToDIP(TrackPoint.X, width,
                ZeroPoint, Units.DIP, Orientation.Horizontal);
            double txInUnits = DipConverter.Convert(tx, ScaleUnits, Orientation.Horizontal);
            double tyInUnits = DipConverter.Convert(TrackPoint.Y, ScaleUnits, Orientation.Vertical);

            var xmarker = FormatText(txInUnits, Brushes.ForestGreen);
            var ymarker = FormatText(tyInUnits, Brushes.ForestGreen);

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

        private void RenderVerticalScale(DrawingContext dc, Pen blackPen)
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
                    Drawline(dc, blackPen, dip, len, Orientation.Vertical);
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
            Drawline(dc, new Pen(Brushes.ForestGreen, onePixelInDip), TrackPoint.Y, width, Orientation.Vertical);

            double txInUnits = DipConverter.Convert(TrackPoint.X, ScaleUnits, Orientation.Vertical);
            double ty = DipConverter.ToDIP(TrackPoint.Y, height,
                ZeroPoint, Units.DIP, Orientation.Vertical);
            double tyInUnits = DipConverter.Convert(ty, ScaleUnits, Orientation.Vertical);

            var xmarker = FormatText(txInUnits, Brushes.ForestGreen);
            var ymarker = FormatText(tyInUnits, Brushes.ForestGreen);
            var measure = FormatText(333.3);

            var xpos = Flip ? width - measure.Width - 2 : 4;
            var ypos1 = TrackPoint.Y - 2 * ymarker.Height - 1 < 0 ?
                TrackPoint.Y + 1 : TrackPoint.Y - 2 * ymarker.Height - 1;
            var ypos2 = TrackPoint.Y - 2 * ymarker.Height - 1 < 0 ?
                TrackPoint.Y + ymarker.Height + 1 : TrackPoint.Y - ymarker.Height - 1;

            dc.DrawText(ymarker, new(xpos, ypos1));
            dc.DrawText(xmarker, new(xpos, ypos2));
        }

        private void Drawline(DrawingContext dc, Pen pen,
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

        private Typeface Typeface => new(FontFamily, FontStyle, FontWeight, FontStretch);

        private FormattedText FormatText(double num)
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

        private FormattedText FormatText(double num, Brush foreground)
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

        private FormattedText FormatText(string text)
        {
            return FormatText(text, Foreground);
        }

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

        private FormattedText FormatFraction(double num, int maxDenominator, Brush foreground)
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

        public static bool Modulo(double a, double b, double maxdelta = 0.5)
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
