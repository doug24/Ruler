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
        }

        [ObservableProperty]
        private Point origin = new();

        [ObservableProperty]
        private Point mousePoint = new();

        [ObservableProperty]
        private Point trackPoint = new();

        [ObservableProperty]
        private Orientation orientation = Orientation.Horizontal;

        [ObservableProperty]
        private Thickness resizeBorder = new(4, 0, 4, 0);

        [ObservableProperty]
        private Units scaleUnits = Units.DIP;

        [ObservableProperty]
        private ZeroPoint zeroPoint = ZeroPoint.Near;

        [ObservableProperty]
        private double longAxis = 800;

        [ObservableProperty]
        private double shortAxis = 80;

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

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Orientation))
            {
                if (Orientation == Orientation.Horizontal)
                {
                    ResizeBorder = new Thickness(4, 0, 4, 0);
                    ActiveEdge = Flip ? Edge.Bottom : Edge.Top;
                    Width = LongAxis;
                    Height = ShortAxis;
                }
                else if (Orientation == Orientation.Vertical)
                {
                    ResizeBorder = new Thickness(0, 4, 0, 4);
                    ActiveEdge = Flip ? Edge.Left : Edge.Right;
                    Width = ShortAxis;
                    Height = LongAxis;
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
            else if (e.PropertyName == nameof(Width))
            {
                if (Orientation == Orientation.Horizontal)
                {
                    LongAxis = Width;
                }
                else if (Orientation == Orientation.Vertical)
                {
                    ShortAxis = Width;
                }
            }
            else if (e.PropertyName == nameof(Height))
            {
                if (Orientation == Orientation.Horizontal)
                {
                    ShortAxis = Height;
                }
                else if (Orientation == Orientation.Vertical)
                {
                    LongAxis = Height;
                }
            }

            base.OnPropertyChanged(e);
        }
    }
}
