using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ruler
{
    public partial class RulerViewModel : ObservableObject
    {
        public RulerViewModel()
        {
            RestoreSettings();

            // after initialization, watch for changes
            PropertyChanging += RulerViewModel_PropertyChanging;
            PropertyChanged += RulerViewModel_PropertyChanged;
        }

        [ObservableProperty]
        private Point origin = new();

        [ObservableProperty]
        private Point mousePoint = new();

        [ObservableProperty]
        private Point trackPoint = new();

        [ObservableProperty]
        private Thickness resizeBorder = new(4, 0, 4, 0);

        [ObservableProperty]
        private Orientation orientation = Orientation.Horizontal;

        [ObservableProperty]
        private Units scaleUnits = Units.DIP;

        [ObservableProperty]
        private ZeroPoint zeroPoint = ZeroPoint.Near;

        [ObservableProperty]
        private double left = 200;

        [ObservableProperty]
        private double top = 400;

        [ObservableProperty]
        private double width = 800;

        [ObservableProperty]
        private double height = 80;

        [ObservableProperty]
        private double shortAxis = 80;

        [ObservableProperty]
        private bool flip = false;

        [ObservableProperty]
        private Edge activeEdge = Edge.Top;

        [ObservableProperty]
        private string scaleToolTip = string.Empty;

        [ObservableProperty]
        private bool angleVisible = false;

        [ObservableProperty]
        private bool perpendicular = true;

        [ObservableProperty]
        private bool magnifierVisible = false;

        [ObservableProperty]
        private double magnifierWidth = 200;

        [ObservableProperty]
        private double magnifierHeight = 200;

        [ObservableProperty]
        private float magnification = 1.6f;

        [ObservableProperty]
        private bool useDefaultFont = true;

        [ObservableProperty]
        private string fontFamily = "Segoe UI";

        [ObservableProperty]
        private double fontSize = 12.0;

        [ObservableProperty]
        private double markerFontSize = 12.0;

        [ObservableProperty]
        private double dialogFontSize = 13.0;

        [ObservableProperty]
        private double opacity = 1;

        [ObservableProperty]
        private bool topMost = false;

        public Theme ColorTheme
        {
            get { return RulerSettings.Default.ColorTheme; }
            set
            {
                SetProperty(RulerSettings.Default.ColorTheme, value, RulerSettings.Default,
                    (s, t) => s.ColorTheme = t);
            }
        }
        public IList<FontInfo> FontFamilies
        {
            get { return Fonts.SystemFontFamilies.Select(r => new FontInfo(r.Source)).OrderBy(f => f.FamilyName).ToList(); }
        }

        public ObservableCollection<double> Markers { get; set; } = new();

        internal void SetMarker()
        {
            if (Orientation == Orientation.Horizontal)
            {
                if (!Markers.Contains(TrackPoint.X))
                {
                    Markers.Add(TrackPoint.X);
                }
            }
            else
            {
                if (!Markers.Contains(TrackPoint.Y))
                {
                    Markers.Add(TrackPoint.Y);
                }
            }
        }

        internal void RemoveMarker()
        {
            List<double> toRemove = new();
            if (Orientation == Orientation.Horizontal)
            {
                foreach (var marker in Markers)
                {
                    if (Math.Abs(marker - TrackPoint.X) <= 1)
                    {
                        toRemove.Add(marker);
                    }
                }
            }
            else
            {
                foreach (var marker in Markers)
                {
                    if (Math.Abs(marker - TrackPoint.Y) <= 1)
                    {
                        toRemove.Add(marker);
                    }
                }
            }

            foreach (var rem in toRemove)
            {
                Markers.Remove(rem);
            }
        }

        internal void ClearMarkers()
        {
            Markers.Clear();
        }

        private void RulerViewModel_PropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            // save current layout values before changing
            if (e.PropertyName == nameof(Orientation))
            {
                SaveCurrentLayout();
            }
        }

        private void RulerViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Orientation))
            {
                RulerSettings.Default.Orientation = Orientation;

                // restore saved values for this orientation
                RestoreCurrentLayout();
                SetBorderProperties();
            }
            else if (e.PropertyName == nameof(Flip))
            {
                if (Orientation == Orientation.Horizontal)
                {
                    ActiveEdge = Flip ? Edge.Bottom : Edge.Top;
                }
                else if (Orientation == Orientation.Vertical)
                {
                    ActiveEdge = Flip ? Edge.Left : Edge.Right;
                }
            }
            else if (e.PropertyName == nameof(ShortAxis))
            {
                if (Orientation == Orientation.Horizontal)
                {
                    Height = ShortAxis;
                }
                else if (Orientation == Orientation.Vertical)
                {
                    Width = ShortAxis;
                }
            }
        }

        private void RestoreSettings()
        {
            Orientation = RulerSettings.Default.Orientation;
            ScaleUnits = RulerSettings.Default.ScaleUnits;
            Magnification = RulerSettings.Default.Magnification;
            Perpendicular = RulerSettings.Default.Perpendicular;
            MagnifierWidth = RulerSettings.Default.MagnifierWidth;
            MagnifierHeight = RulerSettings.Default.MagnifierHeight;
            FontFamily = RulerSettings.Default.FontFamily;
            FontSize = RulerSettings.Default.FontSize;
            MarkerFontSize = RulerSettings.Default.MarkerFontSize;
            DialogFontSize = RulerSettings.Default.DialogFontSize;
            Opacity = RulerSettings.Default.Opacity;
            TopMost = RulerSettings.Default.TopMost;

            var parts = RulerSettings.Default.Markers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var num in parts)
            {
                if (double.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                {
                    Markers.Add(value);
                }
            }

            RestoreCurrentLayout();
            SetBorderProperties();
        }

        internal void SaveSettings()
        {
            RulerSettings.Default.Orientation = Orientation;
            RulerSettings.Default.ScaleUnits = ScaleUnits;
            RulerSettings.Default.Magnification = Magnification;
            RulerSettings.Default.Perpendicular = Perpendicular;
            RulerSettings.Default.MagnifierWidth = MagnifierWidth;
            RulerSettings.Default.MagnifierHeight = MagnifierHeight;
            RulerSettings.Default.FontFamily = FontFamily;
            RulerSettings.Default.FontSize = FontSize;
            RulerSettings.Default.MarkerFontSize = MarkerFontSize;
            RulerSettings.Default.DialogFontSize = DialogFontSize;
            RulerSettings.Default.Opacity = Opacity;
            RulerSettings.Default.TopMost = TopMost;
            RulerSettings.Default.Markers = string.Join(",", Markers.Select(m => m.ToString("G", CultureInfo.InvariantCulture)));

            SaveCurrentLayout();
        }

        private void RestoreCurrentLayout()
        {
            Left = RulerSettings.CurrentLayout.Left;
            Top = RulerSettings.CurrentLayout.Top;
            Width = RulerSettings.CurrentLayout.Width;
            Height = RulerSettings.CurrentLayout.Height;
            ZeroPoint = RulerSettings.CurrentLayout.ZeroPoint;
            Flip = RulerSettings.CurrentLayout.Flip;
        }

        private void SaveCurrentLayout()
        {
            RulerSettings.CurrentLayout.Left = Left;
            RulerSettings.CurrentLayout.Top = Top;
            RulerSettings.CurrentLayout.Width = Width;
            RulerSettings.CurrentLayout.Height = Height;
            RulerSettings.CurrentLayout.ZeroPoint = ZeroPoint;
            RulerSettings.CurrentLayout.Flip = Flip;
        }

        private void SetBorderProperties()
        {
            if (Orientation == Orientation.Horizontal)
            {
                ResizeBorder = new Thickness(4, 0, 4, 0);
                ActiveEdge = Flip ? Edge.Bottom : Edge.Top;
            }
            else if (Orientation == Orientation.Vertical)
            {
                ResizeBorder = new Thickness(0, 4, 0, 4);
                ActiveEdge = Flip ? Edge.Left : Edge.Right;
            }
        }
    }

    public class FontInfo
    {
        public FontInfo(string familyName)
        {
            FamilyName = familyName;
            IsMonospaced = GetIsMonospaced(familyName);
        }
        public string FamilyName { get; private set; }
        public bool IsMonospaced { get; private set; }

        private static bool GetIsMonospaced(string familyName)
        {
            Typeface typeface = new(new FontFamily(familyName), SystemFonts.MessageFontStyle,
                SystemFonts.MessageFontWeight, FontStretches.Normal);

            FormattedText narrowChar = new("i", CultureInfo.CurrentCulture,
                CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                typeface, 12, Brushes.Black, null, 1);
            FormattedText wideChar = new("w", CultureInfo.CurrentCulture,
                CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                typeface, 12, Brushes.Black, null, 1);

            return narrowChar.Width == wideChar.Width;
        }
    }
}
