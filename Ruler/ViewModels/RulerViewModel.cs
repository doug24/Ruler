using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
        private bool flip = false;

        [ObservableProperty]
        private Edge activeEdge = Edge.Top;

        [ObservableProperty]
        private bool angleVisible = false;

        [ObservableProperty]
        private bool magnifierVisible = false;

        [ObservableProperty]
        private float magnification = 1.6f;

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
        }

        private void RestoreSettings()
        {
            Orientation = RulerSettings.Default.Orientation;
            ScaleUnits = RulerSettings.Default.ScaleUnits;
            Magnification = RulerSettings.Default.Magnification;

            RestoreCurrentLayout();
        }

        internal void SaveSettings()
        {
            RulerSettings.Default.Orientation = Orientation;
            RulerSettings.Default.ScaleUnits = ScaleUnits;
            RulerSettings.Default.Magnification = Magnification;

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
    }
}
