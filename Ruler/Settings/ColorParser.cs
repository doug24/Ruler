using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using Config.Net;

namespace Ruler
{
    public class ColorParser : ITypeParser
    {
        public IEnumerable<Type> SupportedTypes => new[] { typeof(SolidColorBrush), typeof(Color) };

        public string? ToRawString(object? value)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color.ToString(CultureInfo.InvariantCulture);
            }
            else if (value is Color color)
            {
                return color.ToString(CultureInfo.InvariantCulture);
            }
            return null;
        }

        public bool TryParse(string? value, Type t, out object? result)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Color color = (Color)ColorConverter.ConvertFromString(value);

                if (t == typeof(Color))
                {
                    result = color;
                    return true;
                }

                if (t == typeof(SolidColorBrush))
                {
                    result = new SolidColorBrush(color);
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
